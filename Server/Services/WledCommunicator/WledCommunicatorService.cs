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

    public WledServer[] WledServers { get; private set; } = [];
    private const double HttpReqCooldownSecs = 0.1;
    private readonly Dictionary<string, DateTime> LastBriHTTPReq = [];
    private readonly Dictionary<LedSegment, DateTime> LastColReq = [];
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
        FillNewSegmentsIntoDatastore();
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

        var secs = (DateTime.Now - LastColReq.GetValueOrDefault(segment)).TotalSeconds;
        if (secs < HttpReqCooldownSecs)
            return false;
        LastColReq[segment] = DateTime.Now;

        if (frequentLogging) logger.WriteLine($"Setting led colors of segment {segment} with resolution of {colors.Length} via UDP...", LogLevel.Debug);

        var host = GetHostFromWledServerAddress(segment.WledServerAddress);
        if (host == null)
        {
            logger.WriteLine($"Could not set led colors over UDP, invalid server address '{segment.WledServerAddress}'", LogLevel.Warn);
            return false;
        }

        try
        {
            WledUdpColorSender.SendSegmentColors(host, segment.Start, SampleColorsToSegment(colors, segment));
            return true;
        }
        catch (Exception e)
        {
            // UDP is fire & forget; a failure here usually just means the server is unreachable right
            // now, and the update loop retries next tick anyway. Only log when frequent logging is on.
            if (frequentLogging) logger.WriteLine(e, LogLevel.Error);
            return false;
        }
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
