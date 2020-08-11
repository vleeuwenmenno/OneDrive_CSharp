using System;
using OneDrive_CSharp;

namespace OneDrive_CSharp_Example
{
    public class Program
    {
        static void Main(string[] args)
        {
            OneDrive od = new OneDrive();

            od.OnTransfer += new OneDriveEvent(onedrive_onTransfer);

            od.authenticate();
            od.startMonitorThread();
        }

        private static void onedrive_onTransfer(object source, OneDriveEventArgs e)
        {
            File val = e.GetFileInfo();
            Console.WriteLine($"{val.job} {val.path} {val.progress}% ({Misc.BytesToString(val.size)}" + (val.progress < 100 ? $"ETA {val.eta.TotalSeconds}s" : "") + ")");
        }
    }
}
