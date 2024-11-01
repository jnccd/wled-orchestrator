using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Server.Helper;
using Server.Services.DataStore;
using Server.Services.DataStore.Types;

namespace Server.Services.WledCommunicator;

[RegisterImplementation(ServiceRegisterType.Singleton, typeof(WledCommunicatorService))]
public interface IWledCommunicatorService
{
    public WledServer[] Leds { get; }

    public void FindLEDs();

    public bool SetBrightnessGlobally(int bri);
    public bool SetBrightnessOnWledServer(int bri, string wledServerAddress);

    public bool SetLedColorsGlobally(Color[] colors);
    public bool SetLedColorsOnWledSegment(Color[] colors, LedSegment segment);
}

public class WledCommunicatorService(
    IDataStoreService dataStore,
    ILoggerService logger)
    : IWledCommunicatorService
{
    public WledServer[] Leds { get; private set; } = [];
    private const double HttpReqCooldownSecs = 0.1;
    private readonly Dictionary<string, DateTime> LastBriHTTPReq = [];
    private readonly Dictionary<LedSegment, DateTime> LastColHTTPReq = [];

    public void FindLEDs()
    {
        logger.WriteLine("Finding local wled servers...");

        var localIp = GetLocalIPAddress()?.GetAddressBytes();
        if (localIp == null)
        {
            return;
        }

        var ledServers = new List<WledServer>();
        var tasks = new List<Task>();
        var fac = new TaskFactory();

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

                if (string.IsNullOrWhiteSpace(responseText) || !responseText.StartsWith("{\"on\":")) return;

                var ledState = JsonConvert.DeserializeObject<WledServerState>(responseText);

                if (ledState != null) ledServers.Add(new(address, ledState));
            }, i));

        Task.WaitAll([.. tasks]);

        logger.WriteLine("Found Wled Servers at: " + ledServers.Select(x => x.Address).Combine(", "));
        Leds = [.. ledServers];
        FillNewSegmentsIntoDatastore();
    }
    public IPAddress? GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
            if (ip.AddressFamily == AddressFamily.InterNetwork)
                return ip;
        return null;
    }
    public void FillNewSegmentsIntoDatastore()
    {
        var defaultGroup = dataStore.Data.Groups.FirstOrDefault(x => x.Name == "Default");
        if (defaultGroup == null)
        {
            defaultGroup = new([], null, "Default", new(255, 255, 255));
            dataStore.Data.Groups.Add(defaultGroup);
        }

        var segments = Leds.SelectMany(x => x.Segments).ToList();

        foreach (var segment in segments)
        {
            var associatedGroup = dataStore.Data.Groups.FirstOrDefault(x => x.LedSegments.Contains(segment)) ?? defaultGroup;
            if (!associatedGroup.LedSegments.Contains(segment)) associatedGroup.LedSegments.Add(segment);
        }

        dataStore.Save();
    }

    public bool SetBrightnessGlobally(int bri)
    {
        foreach (var led in Leds)
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

        logger.WriteLine($"Setting led brightness to {bri} on server {wledServerAddress}...");

        $"{{\"bri\":{bri}}}".HttpPostAsJsonTo($"{wledServerAddress}/json/state");
        return true;
    }

    // https://kno.wled.ge/interfaces/json-api/#per-segment-individual-led-control
    public bool SetLedColorsGlobally(Color[] colors)
    {
        foreach (var led in Leds)
            foreach (var (seg, i) in led.State.Seg.WithIndex())
                if (!SetLedColorsOnWledSegment(colors, new(led.Address, i)))
                    return false;
        return true;
    }
    public bool SetLedColorsOnWledSegment(Color[] colors, LedSegment segment)
    {
        var secs = (DateTime.Now - LastColHTTPReq.GetValueOrDefault(segment)).TotalSeconds;
        if (secs < HttpReqCooldownSecs)
            return false;
        LastColHTTPReq[segment] = DateTime.Now;

        logger.WriteLine($"Setting led colors of segment {segment} with resolution of {colors.Length}...");

        var seg = Leds.FirstOrDefault(l => l.Address == segment.WledServerAddress)?.State.Seg[segment.SegmentIndex];
        if (seg == null || seg.Start == null || seg.Len == null)
        {
            logger.WriteLine($"Segment {segment} does not exist!", LogLevel.Error);
            return false;
        }

        var ledCols = new StringBuilder();
        ledCols.Append("{\"i\":[");
        for (int i = 0; i < seg.Len; i++)
        {
            var col = colors[(int)(i * (float)colors.Length / seg.Len)];
            if (i > 0)
                ledCols.Append(',');
            ledCols.Append($"{seg.Start + i},'{col.ToHex()}'");
        }
        ledCols.Append("]}");

        $"{{\"seg\":{ledCols}}}".HttpPostAsJsonTo($"{segment.WledServerAddress}/json/state");
        return true;
    }
}
