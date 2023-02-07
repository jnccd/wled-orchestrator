using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WledOrchestrator
{
    public static class Extensions
    {
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
            {
                action();
            }
        }
        public static void ForceHide(this Form F)
        {
            Hide(F.Handle);
        }
        public static void ForceShow(this Form F)
        {
            Show(F.Handle);
        }
        public static bool ContainsAll(this string s, string[] contains)
        {
            foreach (string t in contains)
                if (!s.Contains(t))
                    return false;
            return true;
        }
        public static b Foldl<a, b>(this IEnumerable<a> xs, b y, Func<b, a, b> f)
        {
            foreach (a x in xs)
                y = f(y, x);
            return y;
        }
        public static b Foldl<a, b>(this IEnumerable<a> xs, Func<b, a, b> f)
        {
            return xs.Foldl(default, f);
        }
        public static string Combine(this IEnumerable<string> s, string combinator = "")
        {
            return s.Count() == 0 ? "" : s.Foldl("", (x, y) => x + combinator + y).Remove(0, combinator.Length);
        }

        public static async Task<string> GetHttpResponse(this string url, int timeout = 2)
        {
            using var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, timeout);

            using HttpResponseMessage response = await client.GetAsync(url);
            string responseText = await response.Content.ReadAsStringAsync();

            return responseText;
        }
        public static async void HttpPostAsJson(this Dictionary<string, string> dic, string address)
        {
            using var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 2);

            var data = dic;
            var jsonData = JsonConvert.SerializeObject(data);
            var contentData = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var res = await client.PostAsync(address, contentData);
        }

        public static void Hide(IntPtr WindowHandle) { ShowWindow(WindowHandle, 0); }
        public static void Minimize(IntPtr WindowHandle) { ShowWindow(WindowHandle, 2); }
        public static void Show(IntPtr WindowHandle) { ShowWindow(WindowHandle, 5); }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
