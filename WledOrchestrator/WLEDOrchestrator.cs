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
        public class Led 
        { 
            public string address; public 
            LedState state;

            public Led(string address, LedState state)
            {
                this.address = address;
                this.state = state;
            }
        }

        static Led[] Leds = new Led[0];

        public static void FindLEDs()
        {
            var localIp = GetLocalIPAddress().GetAddressBytes();
            if (localIp == null)
                return;

            var leds = new List<Led>();
            var tasks = new List<Task>();
            var fac = new TaskFactory();

            for (int i = 0; i < 256; i++)
                tasks.Add(fac.StartNew(async (i) =>
                {
                    var address = $"http://{localIp[0]}.{localIp[1]}.{localIp[2]}.{i}";

                    try
                    {
                        string responseText = await $"{address}/json/state".GetHttpResponse();

                        if (!responseText.StartsWith("{\"on\":"))
                            return;

                        var ledState = LedState.FromJson(responseText);

                        leds.Add(new Led(address, ledState));
                    }
                    catch (Exception e) { }
                }, i));

            Task.WaitAll(tasks.ToArray());
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
            foreach (var led in Leds)
                new Dictionary<string, string> { { "bri", bri.ToString() } }.
                        HttpPostAsJson($"{led.address}/json/state");
        }

        public static void SetLedColors(Color[] colors)
        {
            foreach (var led in Leds)
                foreach (var seg in led.state.Seg)
                {
                    // TODO: Implement multi segment thingys correctly

                    ColorTranslator.ToHtml(colors[0]);

                    var ledCols = new StringBuilder();
                    ledCols.Append("{\"i\":[");

                    for (int i = 0; i < seg.Len; i++)
                    {
                        var col = colors[i * seg.Len]
                    }

                    ledCols.Append("]}");

                    new Dictionary<string, string> { { "seg", ledCols.ToString() } }.
                        HttpPostAsJson($"{led.address}/json/state");
                }
        }
    }
}
