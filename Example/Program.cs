using System.Threading;
using System;
using OneDrive_CSharp;

namespace OneDrive_CSharp_Example
{
    public class Program
    {
        static OneDrive od;
        static void Main(string[] args)
        {
            od = new OneDrive();

            od.OnTransfer += new OneDriveEvent(onedrive_onTransfer);
            od.OnSyncStatusChanged += new OneDriveEvent(onedrive_onSyncStatusChanged)

            od.authenticate();
            od.startMonitor();
        }

        private static void onedrive_onSyncStatusChanged(object source, OneDriveEventArgs e)
        {
            Console.WriteLine("Syncing status changed to "+ od.isActivelySyncing);
        }

        private static void onedrive_onTransfer(object source, OneDriveEventArgs e)
        {
            File val = e.GetFileInfo();
            Console.WriteLine($"{val.job} {val.path} {val.progress}% ({Misc.BytesToString(val.size)}" + (val.progress < 100 ? $" ETA {val.eta.TotalSeconds}s" : "") + $") (IsActive: {od.isActivelySyncing})");
        }
    }
}
