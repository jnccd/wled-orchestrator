using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Server.Helper;

namespace Server.Services.WledCommunicator;

[AutoRegister(ServiceRegisterType.Singleton, typeof(WledCommunicatorService))]
public interface IWledCommunicatorService
{
    public WledServer[] Leds { get; }

    public WledServer[]? FindLEDs();

    public bool SetBrightness(int bri);

    public bool SetLedColors(Color[] colors);
}

public class WledCommunicatorService : IWledCommunicatorService
{
    public WledServer[] Leds { get; private set; } = [];
    private const double HttpReqCooldownTime = 3;
    private DateTime LastBriHTTPReq = new(1999, 5, 4);
    private DateTime LastColHTTPReq = new(1999, 5, 4);

    public WledServer[]? FindLEDs()
    {
        var localIp = GetLocalIPAddress()?.GetAddressBytes();
        if (localIp == null) return null;

        var leds = new List<WledServer>();
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

                var ledState = JsonConvert.DeserializeObject<LedState>(responseText);

                if (ledState != null) leds.Add(new(address, ledState));
                done++;
            }, i));

        Task.WaitAll([.. tasks]);
        while (done < 250)
            Thread.Sleep(200);

        Debug.WriteLine("Found LEDs at: " + leds.Select(x => x.address).Combine(", "));
        return [.. leds];
    }
    public IPAddress? GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip;
            }
        }
        return null;
    }

    public bool SetBrightness(int bri)
    {
        var secs = (DateTime.Now - LastBriHTTPReq).TotalSeconds;
        if (secs < HttpReqCooldownTime)
            return false;
        LastBriHTTPReq = DateTime.Now;

        foreach (var led in Leds)
            $"{{\"bri\":{bri}}}".HttpPostAsJsonTo($"{led.address}/json/state");
        return true;
    }

    // https://kno.wled.ge/interfaces/json-api/#per-segment-individual-led-control
    public bool SetLedColors(Color[] colors)
    {
        var secs = (DateTime.Now - LastColHTTPReq).TotalSeconds;
        if (secs < HttpReqCooldownTime)
            return false;
        LastColHTTPReq = DateTime.Now;

        foreach (var led in Leds)
            foreach (var seg in led.state.Seg)
            {
                // TODO: Implement multi segment thingys correctly

                var ledCols = new StringBuilder();
                ledCols.Append("{\"i\":[");
                for (int i = 0; i < seg.Len; i++)
                {
                    var col = colors[(int)(i * (float)colors.Length / seg.Len)];
                    if (i > 0)
                        ledCols.Append(',');
                    ledCols.Append($"'{col.ToHex()}'");
                }
                ledCols.Append("]}");

                $"{{\"seg\":{ledCols}}}".HttpPostAsJsonTo($"{led.address}/json/state");
            }
        return true;
    }
}
