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

                var drivenSegments = new List<(LedSegment Segment, LedGroupState State)>();
                foreach (var segment in ledServer)
                {
                    var newLedState = ledThemeProvider.GetNewLedState(segment);
                    if (newLedState != null) drivenSegments.Add((segment, newLedState));
                }

                if (drivenSegments.Count == 0)
                {
                    // Nothing to display on this server; release live mode but leave brightness alone.
                    ReleaseServer(serverAddress);
                    continue;
                }

                int avgBrightness = (int)drivenSegments.Average(s => s.State.Brightness);
                if (avgBrightness <= 0)
                {
                    // Brightness 0: every possible color renders identically (black), so sending
                    // color frames is pointless regardless of what the themes compute.
                    SetBrightnessIfChanged(serverAddress, 0);
                    ReleaseServer(serverAddress);
                    continue;
                }

                foreach (var (segment, newLedState) in drivenSegments)
                    communicatorService.SetLedColorsOnWledSegment([.. newLedState.Colors.Select(x => x.HsvToRgb())], segment);

                SetBrightnessIfChanged(serverAddress, avgBrightness);
                drivenServers.Add(serverAddress);
            }
        }
    }

    void SetBrightnessIfChanged(string serverAddress, int brightness)
    {
        if (lastSentBrightness.GetValueOrDefault(serverAddress) == brightness) return;
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
