using System.Diagnostics;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;
using Server.Services.LedTheme;
using Server.Services.WledCommunicator;
namespace Server.Services;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(UpdaterService))]
public class UpdaterService(
    WledCommunicatorService communicatorService,
    LedThemeProviderService ledThemeProvider,
    DataStoreService dataStore,
    LoggerService logger)
{
    Task? updateTask;
    CancellationTokenSource? cts;
    // Per-LED colors travel over UDP now, so the loop can run at 20fps. Frames are pruned in the
    // communicator when a segments colors are unchanged, and the HTTP brightness request below is
    // only sent when the value actually changes (see lastSentBrightness).
    const int ledUpdateIntervalMillis = 50;

    // When a server has exactly one driven segment, colors whose HSV value is low (dim scenes) are
    // lifted to the full 8-bit range and the uniform dim level is moved into the servers global
    // WLED brightness instead. The brightness is gamma-matched (brightness = level^gamma), so the
    // total light stays identical to the old color-encoded dimming, while the colors themselves
    // keep their full range instead of being crushed into the coarse near-black region of the gamma
    // table. Automatically disabled for servers with several segments, because WLED brightness is
    // global per device and cannot represent several different dim levels at once.
    const bool ConsolidateDimmingIntoBrightness = true;
    // Brightness value last sent to each server, used to avoid spamming identical HTTP requests every tick.
    readonly Dictionary<string, int> lastSentBrightness = [];
    // Servers the update loop currently keeps in WLED realtime mode; they must be explicitly
    // released (UDP "leave live mode" frame) once they are not driven anymore, because WLEDs
    // realtime timeout byte is now 255 (= live mode until told otherwise).
    readonly HashSet<string> drivenServers = [];
    // Servers already settled this process run (driven or explicitly released), to release a WLED
    // only once when it is idle from the start (e.g. left in live mode by a previous process run).
    readonly HashSet<string> settledServers = [];

    public void StartUpdateThread()
    {
        logger.WriteLine("Starting Led Update Loop...");

        cts?.Cancel();
        updateTask?.Dispose();
        cts?.Dispose();
        cts = new CancellationTokenSource();
        updateTask = Task.Run(() =>
        {
            Task.Run(communicatorService.FindLEDs);
            var stopwatch = new Stopwatch();

            while (!cts.Token.IsCancellationRequested)
            {
                stopwatch.Restart();
                try
                {
                    UpdateLedSegments();
                }
                catch (Exception ex)
                {
                    logger.WriteLine(ex, LogLevel.Error);
                }

                int waitMillis = ledUpdateIntervalMillis - (int)stopwatch.ElapsedMilliseconds;
                if (waitMillis > 0)
                    Task.Delay(waitMillis).Wait();
            }
        }, cts.Token);
    }

    private void UpdateLedSegments()
    {
        lock (dataStore.lockject)
        {
            foreach (var ledServer in dataStore.Data.Groups.SelectMany(x => x.LedSegments).GroupBy(x => x.WledServerAddress))
            {
                string serverAddress = ledServer.Key;
                if (!dataStore.Data.Activated)
                {
                    SetBrightnessIfChanged(serverAddress, 0);
                    ReleaseServer(serverAddress);
                    continue;
                }

                var themedSegments = new List<(LedSegment Segment, LedGroupState State)>();
                var unthemedSegments = new List<LedSegment>();
                foreach (var segment in ledServer)
                {
                    var newLedState = ledThemeProvider.GetNewLedState(segment);
                    if (newLedState != null) themedSegments.Add((segment, newLedState));
                    else unthemedSegments.Add(segment); // group has no theme
                }

                if (themedSegments.Count == 0)
                {
                    // No theme drives this server anymore (e.g. the last theme was set to null):
                    // switch the whole server off instead of leaving it on its last colors.
                    SetBrightnessIfChanged(serverAddress, 0);
                    ReleaseServer(serverAddress);
                    continue;
                }

                bool consolidateDimming = ConsolidateDimmingIntoBrightness && themedSegments.Count == 1;
                if (consolidateDimming)
                {
                    var (segment, newLedState) = themedSegments[0];
                    var (colors, brightness) = ConsolidateIntoBrightness(
                        [.. newLedState.Colors.Select(x => x.HsvToRgb())],
                        newLedState.Brightness,
                        communicatorService.GetEffectiveGammaExponent(segment));

                    if (brightness <= 0)
                    {
                        SetBrightnessIfChanged(serverAddress, 0);
                        ReleaseServer(serverAddress);
                        continue;
                    }

                    // The lifted colors are level-free (always up to full range), so the brightness
                    // must reach the device first: otherwise a dimming transition would show the new
                    // colors at the stale high brightness for one frame (bright flash, then black).
                    SetBrightnessIfChanged(serverAddress, brightness);
                    communicatorService.SetLedColorsOnWledSegment(colors, segment);
                    drivenServers.Add(serverAddress);
                }
                else
                {
                    int avgBrightness = (int)themedSegments.Average(s => s.State.Brightness);
                    if (avgBrightness <= 0)
                    {
                        // Brightness 0: every possible color renders identically (black), so sending
                        // color frames is pointless regardless of what the themes compute.
                        SetBrightnessIfChanged(serverAddress, 0);
                        ReleaseServer(serverAddress);
                        continue;
                    }

                    foreach (var (segment, newLedState) in themedSegments)
                        communicatorService.SetLedColorsOnWledSegment([.. newLedState.Colors.Select(x => x.HsvToRgb())], segment);

                    SetBrightnessIfChanged(serverAddress, avgBrightness);
                    drivenServers.Add(serverAddress);
                }

                // Segments whose group has no theme would otherwise keep showing their last frame;
                // clear them (one black frame each, deduped afterwards) so a null theme reads as "off".
                foreach (var segment in unthemedSegments)
                    communicatorService.ClearSegmentColors(segment);
            }
        }
    }

    // Lifts dim colors up to the full 8-bit range and moves the removed level into the (global)
    // WLED brightness. The themes HSV values stay the ground truth - only their *level* is split
    // off, so dim scenes keep hue/channel resolution instead of being crushed into the coarse
    // near-black region of the gamma table.
    //
    // The brightness is gamma-matched to keep the total light output identical: lifting the colors
    // by 1/level multiplies each channel by level^-gamma in the gamma domain, so the brightness is
    // reduced by level^gamma (with level = maxChannel/255) to compensate.
    static (ColorRgb[] Colors, int Brightness) ConsolidateIntoBrightness(
        ColorRgb[] colors, int brightness, double gammaExponent)
    {
        if (colors.Length == 0 || brightness <= 0)
            return (colors, brightness);

        int maxChannel = 0;
        foreach (var color in colors)
            maxChannel = Math.Max(maxChannel, Math.Max(color.R, Math.Max(color.G, color.B)));

        if (maxChannel <= 1 || maxChannel >= 255)
            return (colors, brightness); // all black or already full range: nothing to consolidate

        double scale = 255.0 / maxChannel;
        var lifted = new ColorRgb[colors.Length];
        for (int i = 0; i < colors.Length; i++)
            lifted[i] = new ColorRgb(
                (byte)Math.Min(255, (int)Math.Round(colors[i].R * scale)),
                (byte)Math.Min(255, (int)Math.Round(colors[i].G * scale)),
                (byte)Math.Min(255, (int)Math.Round(colors[i].B * scale)));

        if (gammaExponent <= 0.1)
            return (lifted, brightness); // no color curve in play: keep the plain level

        double level = maxChannel / 255.0;
        double dimmed = brightness * Math.Pow(level, gammaExponent);
        int dimmedBrightness = dimmed <= 0.5 ? 0 : Math.Clamp((int)Math.Round(dimmed), 1, 255);
        return (lifted, dimmedBrightness);
    }

    void SetBrightnessIfChanged(string serverAddress, int brightness)
    {
        // Post on the first decision for a server too (e.g. right after a restart, when the device
        // may still be at an arbitrary brightness), not only when the value differs from before.
        if (lastSentBrightness.TryGetValue(serverAddress, out var lastSent) && lastSent == brightness) return;
        communicatorService.SetBrightnessOnWledServer(brightness, serverAddress);
        lastSentBrightness[serverAddress] = brightness;
    }

    void ReleaseServer(string serverAddress)
    {
        // Tell the WLED to leave live mode once: when we were actively driving it, or the first time
        // we settle this server this run (covers WLEDs left in live mode by a previous process run).
        if (drivenServers.Remove(serverAddress) || !settledServers.Contains(serverAddress))
        {
            communicatorService.CancelRealtimeOnWledServer(serverAddress);
            settledServers.Add(serverAddress);
        }
    }
}
