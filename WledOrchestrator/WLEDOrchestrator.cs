using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Policy;

namespace WledOrchestrator
{
    public static class WLEDOrchestrator
    {
        public static List<string> FindLEDs()
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
                    if ((int)i == 33)
                        localIp.GetHashCode();

                    var address = $"http://{localIp[0]}.{localIp[1]}.{localIp[2]}.{i}/json/state";

                    try
                    {
                        HttpClient client = new HttpClient();
                        client.Timeout = new TimeSpan(0, 0, 1);

                        using HttpResponseMessage response = await client.GetAsync(address);
                        using HttpContent content = response.Content;
                        string responseText = await content.ReadAsStringAsync();

                        Debug.WriteLine(responseText);
                        if (!responseText.StartsWith("{\"on\":"))
                            return;

                        ledAddresses.Add(address);
                        Debug.WriteLine("Found LED at: " + address);
                    }
                    catch (Exception e) 
                    { 
                    }
                }, i));

            Task.WaitAll(tasks.ToArray());
            Debug.WriteLine("Found LEDs at: " + ledAddresses.Combine(", "));
            return ledAddresses;
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
