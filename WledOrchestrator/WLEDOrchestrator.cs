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
        static string[] LedAddresses = new string[0];

        public static void Init()
        {
            LedAddresses = FindLEDs();
        }

        public static void SetGlobalBrightness(int bri)
        {
            Task t = Task.Run(async () =>
            {
                foreach (var led in LedAddresses)
                    await new Dictionary<string, string> { { "bri", bri.ToString() } }.
                            HttpPostAsJson($"{led}/json/state");
            });
            t.Wait();
        }

        public static string[] FindLEDs()
        {
            var localIp = GetLocalIPAddress().GetAddressBytes();
            if (localIp == null)
                return null;

            var ledAddresses = new List<string>();
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

                        ledAddresses.Add(address);
                    }
                    catch (Exception e)  {  }
                }, i));

            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("Found LEDs at: " + ledAddresses.Combine(", "));
            return ledAddresses.ToArray();
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
    }
}
