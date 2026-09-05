using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;

namespace Server.Services.WledCommunicator;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(WledCommunicatorService))]
public class WledCommunicatorService(
    DataStoreService dataStore,
    LoggerService logger)
{
    public record WledServer(string Address, WledServerState State);

    /// <summary>Last colors successfully sent for a segment, used to prune duplicate UDP frames.</summary>
    class CachedSegmentColors(ColorRgb[] colors, DateTime sentAt)
    {
        public ColorRgb[] Colors { get; } = colors;
        public DateTime SentAt { get; } = sentAt;
    }

    public WledServer[] WledServers { get; private set; } = [];
    private const double HttpReqCooldownSecs = 0.1;
    private readonly Dictionary<string, DateTime> LastBriHTTPReq = [];
    private readonly Dictionary<LedSegment, DateTime> LastColReq = [];

    // Cache of the last successfully sent colors per segment. Frames are UDP realtime with an
    // infinite timeout byte (255), so an unchanged picture does not need re-sending every tick.
    readonly Dictionary<LedSegment, CachedSegmentColors> lastSentColors = [];
    // A static picture is re-sent after this interval even when unchanged: a WLED that rebooted or
    // left live mode on its own has no way to tell us, and the resend re-syncs it cheaply. 2s keeps
    // interactive color edits responsive if a device drops out of live mode without us noticing.
    static readonly TimeSpan ResendStaleColorsInterval = TimeSpan.FromSeconds(2);

    private bool frequentLogging = false;

    public void FindLEDs()
    {
        logger.WriteLine("Finding local wled servers...");

        var localIp = GetLocalIPAddress()?.GetAddressBytes();
        if (localIp == null)
        {
            return;
        }

        var wledServers = new List<WledServer>();
        var tasks = new List<Task>();
        var fac = new TaskFactory();
        int done = 0;

        for (int i = 0; i < 256; i++)
            tasks.Add(fac.StartNew(async (i) =>
            {
                var address = $"http://{localIp[0]}.{localIp[1]}.{localIp[2]}.{i}";

                string responseText = "";
                try
                {
                    responseText = await $"{address}/json/state".GetHttpResponseFrom();
                }
                catch (Exception) { }

                if (string.IsNullOrWhiteSpace(responseText) || !responseText.StartsWith("{\"on\":"))
                {
                    done++;
                    return;
                }

                var ledState = JsonConvert.DeserializeObject<WledServerState>(responseText);

                if (ledState != null) wledServers.Add(new(address, ledState));
                done++;
            }, i));

        Task.WaitAll([.. tasks]);
        while (done < 240)
            Thread.Sleep(200);

        logger.WriteLine("Found Wled Servers at: " + wledServers.Select(x => x.Address).Combine(", "));
        WledServers = [.. wledServers];
        LoadColorCorrections();
        FillNewSegmentsIntoDatastore();
    }

    /// <summary>
    /// Per-server color correction settings (gamma curve + whether the server corrects realtime data
    /// itself), read once per discovery from each servers /json/cfg. WLED skips its own gamma
    /// correction for UDP realtime frames by default, so these settings are used to pre-apply the same
    /// correction the old JSON per-LED transport used to get (see <see cref="WledColorCorrection"/>).
    /// </summary>
    public Dictionary<string, WledColorCorrection> ColorCorrections { get; private set; } = [];

    void LoadColorCorrections()
    {
        ColorCorrections = [];
        foreach (var server in WledServers)
        {
            string address = server.Address;
            try
            {
                var cfgJson = $"{address}/json/cfg".GetHttpResponseFrom().GetAwaiter().GetResult();
                ColorCorrections[address] = WledColorCorrection.FromCfgJson(cfgJson) ?? WledColorCorrection.DefaultFallback;
            }
            catch
            {
                ColorCorrections[address] = WledColorCorrection.DefaultFallback;
            }
        }
    }
    static IPAddress? GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var interNetworkAddresses = host.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork).OrderByDescending(x =>
        {
            var score = 0;
            var address = x.GetAddressBytes();

            if (address[0] == 192)
                score += 10;

            if (address[3] != 1)
                score += 1;

            return score;
        });
        return interNetworkAddresses.FirstOrDefault();
    }
    void FillNewSegmentsIntoDatastore()
    {
        lock (dataStore.lockject)
        {
            var defaultGroup = dataStore.Data.Groups.FirstOrDefault();
            if (defaultGroup == null)
            {
                logger.WriteLine("Creating new Default LedSegmentGroup!", LogLevel.Warn);
                defaultGroup = LedSegmentGroup.DefaultGroup;
                dataStore.Data.Groups.Add(defaultGroup);
            }

            var segments = WledServers.SelectMany(server => server.State.Seg.Select(seg => WledSegToNewLedSegment(server.Address, seg))).ToList();

            foreach (var segment in segments)
            {
                var associatedGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(segment)) ?? defaultGroup;
                if (!associatedGroup.LedSegments.Contains(segment))
                    associatedGroup.LedSegments.Add(segment);
                else
                {
                    // Set seg values in existing entry
                    var segmentInGroup = associatedGroup.LedSegments.FirstOrDefault(x => x.Id == segment.Id);
                    if (segmentInGroup == null)
                    {
                        logger.WriteLine("Segment is weird", LogLevel.Error);
                        continue;
                    }
                    segmentInGroup.Start = segment.Start;
                    segmentInGroup.Length = segment.Length;
                }
            }

            dataStore.Save();
        }
    }

    LedSegment WledSegToNewLedSegment(string WledServerAddress, Seg wledSeg) =>
        new LedSegment(WledServerAddress, (int)(wledSeg.Id ?? 0), (int)(wledSeg.Start ?? 0), (int)(wledSeg.Len ?? 0));
    LedSegment WledSegToLedSegment(string WledServerAddress, Seg wledSeg)
    {
        var preliminaryLedSegment = WledSegToNewLedSegment(WledServerAddress, wledSeg);
        var segmentInDatastore = LedSegment.FindInDatastore(preliminaryLedSegment.Id, dataStore);
        if (segmentInDatastore != null)
            return segmentInDatastore;
        else
            return preliminaryLedSegment;
    }

    public bool SetBrightnessGlobally(int bri)
    {
        foreach (var led in WledServers)
            if (!SetBrightnessOnWledServer(bri, led.Address))
                return false;
        return true;
    }
    public bool SetBrightnessOnWledServer(int bri, string wledServerAddress)
    {
        var secs = (DateTime.Now - LastBriHTTPReq.GetValueOrDefault(wledServerAddress)).TotalSeconds;
        if (secs < HttpReqCooldownSecs)
            return false;
        LastBriHTTPReq[wledServerAddress] = DateTime.Now;

        if (frequentLogging) logger.WriteLine($"Setting led brightness to {bri} on server {wledServerAddress}...", LogLevel.Debug);

        $"{{\"bri\":{bri}}}".HttpPostAsJsonTo($"{wledServerAddress}/json/state");
        return true;
    }

    public bool SetLedColorsGlobally(ColorRgb[] colors)
    {
        foreach (var wledServer in WledServers)
            foreach (var segment in wledServer.State.Seg.Select(x => WledSegToLedSegment(wledServer.Address, x)))
                if (!SetLedColorsOnWledSegment(colors, segment))
                    return false;
        return true;
    }
    // https://kno.wled.ge/interfaces/udp-realtime/
    // Default transport of the update loop: sends the per-LED colors through WLEDs "UDP realtime"
    // (DNRGB) channel. That channel shares the UDP port of WLEDs sync notifier (default 21324),
    // making it far cheaper than the per-LED JSON POSTs the HTTP variant used.
    // The old JSON transport stays available as SetLedColorsOnWledSegmentHttpJson (e.g. for servers
    // that have UDP realtime disabled), and custom per-server UDP ports can be added later.
    public bool SetLedColorsOnWledSegment(ColorRgb[] colors, LedSegment segment)
    {
        if (colors.Length == 0 || segment.Length == 0)
            return false;

        // Sample the theme colors down to one color per physical LED (same mapping the old JSON
        // transport used). This is also the value the duplicate check below compares against.
        return SendSegmentColorsUdp(segment, SampleColorsToSegment(colors, segment));
    }

    /// <summary>
    /// Paints a whole segment black ("off"). Used for segments whose group has no theme, so they do
    /// not keep showing their last colors. The dedup cache keeps this to a single frame per segment
    /// until the colors would change again.
    /// </summary>
    public bool ClearSegmentColors(LedSegment segment)
    {
        if (segment.Length == 0)
            return false;

        return SendSegmentColorsUdp(segment, GetBlackColors(segment.Length));
    }

    // Reusable black pixel arrays, one per segment length, so clearing a segment does not allocate a
    // new array every tick. Entries are never mutated by callers.
    static readonly Dictionary<int, ColorRgb[]> blackColorsCache = [];
    static ColorRgb[] GetBlackColors(int length)
    {
        lock (blackColorsCache)
        {
            if (!blackColorsCache.TryGetValue(length, out var black))
            {
                black = new ColorRgb[length];
                for (int i = 0; i < length; i++)
                    black[i] = new ColorRgb(0, 0, 0);
                blackColorsCache[length] = black;
            }
            return black;
        }
    }

    // Sends one segment frame over UDP unless the colors are unchanged since the last successful
    // send. No cooldown, but duplicate frames are pruned: WLEDs realtime timeout byte is 255 (stay
    // in live mode indefinitely), so an unchanged picture needs no further traffic. Returns false
    // => no frame was sent.
    bool SendSegmentColorsUdp(LedSegment segment, ColorRgb[] sampled)
    {
        if (lastSentColors.TryGetValue(segment, out var last) && SameColors(last.Colors, sampled)
            && DateTime.Now - last.SentAt < ResendStaleColorsInterval)
            return false;

        if (frequentLogging) logger.WriteLine($"Setting led colors of segment {segment} with resolution of {sampled.Length} via UDP...", LogLevel.Debug);

        var host = GetHostFromWledServerAddress(segment.WledServerAddress);
        if (host == null)
        {
            logger.WriteLine($"Could not set led colors over UDP, invalid server address '{segment.WledServerAddress}'", LogLevel.Warn);
            return false;
        }

        try
        {
            var correction = ColorCorrections.GetValueOrDefault(segment.WledServerAddress) ?? WledColorCorrection.DefaultFallback;
            WledUdpColorSender.SendSegmentColors(host, segment.Start, sampled, gammaLut: correction.GammaLut);
            lastSentColors[segment] = new(sampled, DateTime.Now);
            return true;
        }
        catch (Exception e)
        {
            lastSentColors.Remove(segment); // dont trust stale colors; retry on a later tick
            // UDP is fire & forget; a failure here usually just means the server is unreachable right
            // now, and the update loop retries next tick anyway. Only log when frequent logging is on.
            if (frequentLogging) logger.WriteLine(e, LogLevel.Error);
            return false;
        }
    }

    /// <summary>
    /// Gamma exponent applied to this servers colors (the devices own gamma or
    /// <see cref="WledColorCorrection.GammaExponentOverride"/>). Used when dim colors are split into
    /// the global brightness so that the total light output stays identical.
    /// </summary>
    public double GetEffectiveGammaExponent(string wledServerAddress) =>
        (ColorCorrections.GetValueOrDefault(wledServerAddress) ?? WledColorCorrection.DefaultFallback).EffectiveGammaExponent;

    /// <summary>
    /// Releases a WLED server from realtime mode (timeout byte 255 keeps it in live mode forever
    /// otherwise) and clears its dedup cache, so the next tick that drives the server repaints it.
    /// Called when the server is deactivated or has nothing left to display.
    /// </summary>
    public void CancelRealtimeOnWledServer(string wledServerAddress)
    {
        foreach (var cached in lastSentColors.Keys.Where(x => x.WledServerAddress == wledServerAddress).ToArray())
            lastSentColors.Remove(cached);

        var host = GetHostFromWledServerAddress(wledServerAddress);
        if (host == null) return;

        try
        {
            WledUdpColorSender.CancelRealtime(host);
        }
        catch
        {
            // Ignore; the server is unreachable right now, the regular update flow retries later.
        }
    }

    static bool SameColors(ColorRgb[] a, ColorRgb[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++)
            if (a[i].R != b[i].R || a[i].G != b[i].G || a[i].B != b[i].B)
                return false;
        return true;
    }

    // https://kno.wled.ge/interfaces/json-api/#per-segment-individual-led-control
    // Old transport of the update loop: per-LED colors via HTTP JSON POST. Kept as an explicit
    // fallback for servers without UDP realtime enabled.
    public bool SetLedColorsOnWledSegmentHttpJson(ColorRgb[] colors, LedSegment segment)
    {
        if (colors.Length == 0 || segment.Length == 0)
            return false;

        var secs = (DateTime.Now - LastColReq.GetValueOrDefault(segment)).TotalSeconds;
        if (secs < HttpReqCooldownSecs)
            return false;
        LastColReq[segment] = DateTime.Now;

        if (frequentLogging) logger.WriteLine($"Setting led colors of segment {segment} with resolution of {colors.Length} via HTTP...", LogLevel.Debug);

        var ledCols = new StringBuilder();
        ledCols.Append("{\"i\":[");
        for (int i = 0; i < segment.Length; i++)
        {
            var col = colors[(int)(i * (float)colors.Length / segment.Length)];
            if (i > 0)
                ledCols.Append(',');
            ledCols.Append($"{segment.Start + i},'{col.ToHex()}'");
        }
        ledCols.Append("]}");

        $"{{\"seg\":{ledCols}}}".HttpPostAsJsonTo($"{segment.WledServerAddress}/json/state");
        return true;
    }

    // Converts the theme color array (any resolution) into one color per physical LED of the segment,
    // using the same sampling the old HTTP JSON transport used.
    static ColorRgb[] SampleColorsToSegment(ColorRgb[] colors, LedSegment segment) =>
        Enumerable.Range(0, segment.Length)
            .Select(i => colors[(int)(i * (float)colors.Length / segment.Length)])
            .ToArray();

    // WledServerAddresses are stored as full urls ("http://192.168.x.y"), extract the host part for UDP.
    static string? GetHostFromWledServerAddress(string wledServerAddress)
    {
        if (Uri.TryCreate(wledServerAddress, UriKind.Absolute, out var uri) && !string.IsNullOrWhiteSpace(uri.Host))
            return uri.Host;
        // Fall back to treating the address as a bare ip/hostname
        return wledServerAddress.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
    }
}
