using System;
using System.Diagnostics;

namespace OneDrive_CSharp
{
    public class Misc
    {
        public static Process unixProc;
        public static void unix_proc(string exec, string parameter, Action<string> callback = null, bool callbackPerLine = true)
        {
            unixProc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exec,
                    Arguments = $"{parameter}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            unixProc.Start();
            string output = "";
            while (!unixProc.StandardOutput.EndOfStream)
            {
                string line = unixProc.StandardOutput.ReadLine();

                if (callback != null && callbackPerLine)
                    callback(line);

                output += line + Environment.NewLine;
            }

            if (callback != null && !callbackPerLine)
                callback(output);
        }

        public static void unix(string exec, string parameter, Action<string> callback = null, bool callbackPerLine = true)
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exec,
                    Arguments = $"{parameter}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = "";
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();

                if (callback != null && callbackPerLine)
                    callback(line);

                output += line + Environment.NewLine;
            }

            if (callback != null && !callbackPerLine)
                callback(output);
        }

        public static string unix_simple(string exec, string parameter, bool killSoon = false, string input = "")
        {
            Process proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exec,
                    Arguments = $"{parameter}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = input != "",
                    CreateNoWindow = true
                }
            };

            proc.Start();
            string output = "";

            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                output += line + Environment.NewLine;

                if (input != "")
                {
                    proc.StandardInput.WriteLine(input);
                    input = "";
                }

                if (killSoon)
                    proc.Kill();
            }
            return output;
        }

        public static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + " " + suf[place];
        }
    }
}