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
        private static int RunAsConsoleCommandThreadIndex = 0;

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
        public static void RunAsConsoleCommand(this string command, int TimeLimitInSeconds, Action TimeoutEvent, Action<string, string> ExecutedEvent,
            Action<StreamWriter> RunEvent = null, string WorkingDir = "", Process compiler = null)
        {
            bool exited = false;
            string[] split = command.Split(' ');

            if (split.Length == 0)
                return;

            if (compiler == null)
                compiler = new Process();

            using (compiler)
            {
                compiler.StartInfo.FileName = split.First();
                compiler.StartInfo.Arguments = split.Skip(1).Foldl("", (x, y) => x + " " + y).Trim(' ');
                compiler.StartInfo.CreateNoWindow = true;
                compiler.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                compiler.StartInfo.RedirectStandardInput = true;
                compiler.StartInfo.RedirectStandardOutput = true;
                compiler.StartInfo.RedirectStandardError = true;
                if (!string.IsNullOrWhiteSpace(WorkingDir))
                    compiler.StartInfo.WorkingDirectory = WorkingDir;
                compiler.Start();

                Task.Run(() => { RunEvent?.Invoke(compiler.StandardInput); });

                DateTime start = DateTime.Now;

                Task.Run(() =>
                {
                    Thread.CurrentThread.Name = $"RunAsConsoleCommand Thread {RunAsConsoleCommandThreadIndex++}";
                    compiler.WaitForExit();

                    string o = "";
                    string e = "";

                    try { o = compiler.StandardOutput.ReadToEnd(); } catch { }
                    try { e = compiler.StandardError.ReadToEnd(); } catch { }

                    exited = true;
                    ExecutedEvent(o, e);
                });

                while (!exited && (DateTime.Now - start).TotalSeconds < TimeLimitInSeconds)
                    Thread.Sleep(100);
                if (!exited)
                {
                    exited = true;
                    try
                    {
                        compiler.Close();
                    }
                    catch { }
                    TimeoutEvent();
                }
            }
        }
        public static string GetShellOut(this string command, int timeout = 10000)
        {
            string[] split = command.Split(' ');

            Process P = Process.Start(new ProcessStartInfo()
            {
                FileName = split.First(),
                Arguments = split.Skip(1).Combine(" "),
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            });
            if (!P.WaitForExit(timeout))
                P.Kill();

            return P.StandardOutput.ReadToEnd() + P.StandardError.ReadToEnd();
        }
        public static string GetHTMLfromURL(this string URL, int timeout = 10000, bool silentError = false)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = timeout;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";
                using (WebResponse w = req.GetResponse())
                using (Stream s = w.GetResponseStream())
                using (StreamReader sr = new StreamReader(s))
                    return sr.ReadToEnd();
            }
            catch (Exception e) 
            {   
                if (silentError)
                {
                    return "";
                }
                else
                {
                    return $"Exception: {e}";
                }
            }
        }
        public static WebResponse GetWebResponsefromURL(this string URL)
        {
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                req.KeepAlive = false;
                req.Timeout = 3000;
                req.AllowAutoRedirect = true;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0";

                return req.GetResponse();
            }
            catch (Exception) { return null; }
        }
        public static MemoryStream GetStreamFromUrl(this string url)
        {
            byte[] imageData = null;
            MemoryStream ms = null;

            try
            {
                using (var wc = new WebClient())
                    imageData = wc.DownloadData(url);
                ms = new MemoryStream(imageData);
            }
            catch { }

            return ms;
        }

        public static void Hide(IntPtr WindowHandle) { ShowWindow(WindowHandle, 0); }
        public static void Minimize(IntPtr WindowHandle) { ShowWindow(WindowHandle, 2); }
        public static void Show(IntPtr WindowHandle) { ShowWindow(WindowHandle, 5); }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
