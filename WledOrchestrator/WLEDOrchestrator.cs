using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Policy;
using System.Threading;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace WledOrchestrator
{
    public static class WLEDOrchestrator
    {
        public static Led[] Leds = new Led[0];
        static DateTime LastHTTPReq = new DateTime(1999,5,4);
        static double HttpReqCooldownTime = 3;

        public class Led
        { 
            public string address; 
            public LedState state;

            public Led(string address, LedState state)
            {
                this.address = address;
                this.state = state;
            }
        }

        public static void FindLEDs()
        {
            var localIp = GetLocalIPAddress().GetAddressBytes();
            if (localIp == null)
                return;

            var leds = new List<Led>();
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
                    } catch (Exception e) { }

                    if (string.IsNullOrWhiteSpace(responseText) || !responseText.StartsWith("{\"on\":"))
                    {
                        done++;
                        return;
                    }

                    var ledState = JsonConvert.DeserializeObject<LedState>(responseText);

                    leds.Add(new Led(address, ledState));
                    done++;
                }, i));

            Task.WaitAll(tasks.ToArray());
            while (done < 250)
                Thread.Sleep(200);

            Debug.WriteLine("Found LEDs at: " + leds.Select(x => x.address).Combine(", "));
            Leds = leds.ToArray();
        }
        public static IPAddress GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public static void SetGlobalBrightness(int bri)
        {
            if ((LastHTTPReq - DateTime.Now).TotalSeconds < HttpReqCooldownTime)
                return;
            LastHTTPReq = DateTime.Now;

            foreach (var led in Leds)
                $"{{\"bri\":{bri}}}".HttpPostAsJsonTo($"{led.address}/json/state");
        }

        public static void SetLedColors(Color[] colors)
        {
            if ((LastHTTPReq - DateTime.Now).TotalSeconds < HttpReqCooldownTime)
                return;
            LastHTTPReq = DateTime.Now;

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
        }
    }
}
