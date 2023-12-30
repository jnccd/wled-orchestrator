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

        public static string ToHex(this Color c) => $"{c.R:X2}{c.G:X2}{c.B:X2}";
        public static Color Lerp(this Color c, Color b, double x) =>
            Color.FromArgb((int)(c.R * (1 - x) + b.R * x),
                           (int)(c.G * (1 - x) + b.G * x),
                           (int)(c.B * (1 - x) + b.B * x));

        public static async Task<string> GetHttpResponseFrom(this string url, int timeout = 5)
        {
            using var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, timeout);

            using HttpResponseMessage response = await client.GetAsync(url);
            string responseText = await response.Content.ReadAsStringAsync();

            return responseText;
        }
        public static async void HttpPostAsJsonTo(this string json, string address, int timeout = 5)
        {
            using var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, timeout);

            var contentData = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var res = await client.PostAsync(address, contentData);
            }
            catch (TaskCanceledException)
            {

            }
        }

        public static void Hide(IntPtr WindowHandle) { ShowWindow(WindowHandle, 0); }
        public static void Minimize(IntPtr WindowHandle) { ShowWindow(WindowHandle, 2); }
        public static void Show(IntPtr WindowHandle) { ShowWindow(WindowHandle, 5); }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
