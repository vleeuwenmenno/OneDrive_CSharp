using System.Linq;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;

namespace OneDrive_CSharp
{
    public delegate void OneDriveEvent(object source, OneDriveEventArgs e);

    //This is a class which describes the event to the class that recieves it.
    //An EventArgs class must always derive from System.EventArgs.
    public class OneDriveEventArgs : EventArgs
    {
        private File fileInfo;

        public OneDriveEventArgs(File file)
        {
            fileInfo = file;
        }

        public File GetFileInfo()
        {
            return fileInfo;
        }
    }

    public class OneDrive
    {
        public Dictionary<string, File> files { get; private set; }
        public int filesMaxSize = 100;

        public DateTime lastUpdate {get; private set;}

        public string configRoot { get; private set; }

        public OneDriveEvent OnTransfer;
        public OneDriveEvent OnSyncStatusChanged;
        public OneDriveEvent OnHeartBeat;

        private string syncRoot { get { return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/OneDrive/"; } }

        private Thread odMonitor;

        public bool hasAuthenticated { get; private set; }

        private bool verbose = false;

        private bool _isActivelySyncing = false;

        private System.Timers.Timer heartBeat;
        private bool lastUpdateSync = true;

        public bool isActivelySyncing 
        {
            get
            {
                if (DateTime.Now.Subtract(lastUpdate) >= TimeSpan.FromSeconds(3) && files.Count == 0)
                    _isActivelySyncing = false;
                else if (DateTime.Now.Subtract(lastUpdate) >= TimeSpan.FromSeconds(3) && files.Last().Value.progress == 100)
                    _isActivelySyncing = false;
                else if (DateTime.Now.Subtract(lastUpdate) < TimeSpan.FromSeconds(3))
                    _isActivelySyncing = true;
                else if (DateTime.Now.Subtract(lastUpdate) >= TimeSpan.FromSeconds(3) && files.Last().Value.progress < 100)
                    _isActivelySyncing = true;

                return _isActivelySyncing;
            }
        }

        public void stopMonitor()
        {
            odMonitor.Abort();
            heartBeat.Stop();
        }

        /// <summary>
        /// Starts monitor in a new thread
        /// </summary>
        public void startMonitorThread()
        {
            if (odMonitor == null)
            {
                if (!hasAuthenticated)
                    throw new Exception("OneDrive has not been authenticated, run authenticate() before starting sync or monitoring!");

                odMonitor = new Thread(() =>
                {
                    heartBeat = new System.Timers.Timer();
                    heartBeat.Interval = 3000;
                    heartBeat.Elapsed += heartBeat_OnElapsed;
                    heartBeat.Enabled = true;
                    heartBeat.Start();
                    
                    // Run onedrive monitor and hide the notifications of it
                    Misc.unix("onedrive", $"-m --disable-notifications --confdir=\"{configRoot}\"", monitorCallback, true);
                });
                odMonitor.Start();
            }
            else
                throw new Exception("OneDrive monitor is already running ...");
        }

        private void heartBeat_OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (OnSyncStatusChanged != null)
            {
                if (isActivelySyncing != lastUpdateSync)
                {
                    OnSyncStatusChanged(this, null);
                    lastUpdateSync = isActivelySyncing;
                }
            }

            if (OnHeartBeat != null)
                OnHeartBeat(this, null);
        }

        /// <summary>
        /// Start monitor and wait until its exited
        /// </summary>
        public void startMonitor()
        {
            if (odMonitor == null)
            {
                if (!hasAuthenticated)
                    throw new Exception("OneDrive has not been authenticated, run authenticate() before starting sync or monitoring!");

                heartBeat = new System.Timers.Timer();
                heartBeat.Interval = 5000;
                heartBeat.Elapsed += heartBeat_OnElapsed;
                heartBeat.Enabled = true;
                heartBeat.Start();

                // Run onedrive monitor and hide the notifications of it
                Misc.unix("onedrive", $"-m --disable-notifications --confdir=\"{configRoot}\"", monitorCallback, true);
            }
            else
                throw new Exception("OneDrive monitor is already running ...");
        }

        public void authenticate()
        {
            string s = Misc.unix_simple("onedrive", "", true);
            if (s.StartsWith("Authorize this app visiting:"))
            {
                Misc.unix_simple("xdg-open", s.Replace("Authorize this app visiting:\n\n", "").Replace("\n\nEnter the response uri: \n", ""));

                string res = Misc.unix_simple("zenity", "--forms --title=\"Login to your Microsoft Account\" --text=\"Please login to your Microsoft account to continue.\\nOnce logged in you will be redirected to a blank page, copy the address url and paste it down below.\" --add-entry=\"Authentication URL\"");
                Console.WriteLine(Misc.unix_simple("onedrive", "", false, res.Replace("\n", "")));
            }

            hasAuthenticated = true;
        }

        private void shrinkList()
        {
            if (files.Count >= filesMaxSize)
            {
                Console.WriteLine("History exceeding limits, stripping out oldest 5 items ... ");
                for (int i = 5; i > 0; i--)
                    files.Remove(files.First().Key);

                GC.Collect();
            }
        }

        private void monitorCallback(string line)
        {
            if (string.IsNullOrEmpty(line.Trim()))
                return;

            if (verbose)
                Console.WriteLine($"VERBOSE: {line}");

            shrinkList();

            if (line.Trim() == "done.")
            {
                File f = files.Last().Value;
                files.Remove(files.Last().Key);
                
                f.done = true;
                f.progress = 100;
                f.eta = new TimeSpan(0,0,0);
                f.size = System.IO.File.Exists(syncRoot + f.path) ? new System.IO.FileInfo(syncRoot + f.path).Length : 0;

                files.Add(f.path, f);
                Console.WriteLine($"{f.job} {f.path} {f.progress}% ({Misc.BytesToString(f.size)})");
            }

            RegexOptions options = RegexOptions.Multiline;

            #region Check if it's a progress (Downloading/Uploading)
            string progressPattern = @"^(?<job>[a-zA-Z]{9,11})[\s]{1,4}(?<progress>[0-9]{1,2})\%[\s]{1,4}\|(?<progressBar>.*)\|[\s]{1,4}([ETA]{3})[\s]{1,4}(?<eta>[0-9]{2}:[0-9]{2}:[0-9]{2}|(--:--:--:))";
            foreach (Match m in Regex.Matches(line, progressPattern, options))
            {
                KeyValuePair<string, File> val = files.Last();

                val.Value.progress = double.Parse(m.Groups["progress"].Value);
                val.Value.job = m.Groups["job"].Value == "Downloading" ? JobType.Downloading : m.Groups["job"].Value == "Uploading" ? JobType.Uploading : JobType.Deleting;
                val.Value.size = System.IO.File.Exists(syncRoot + val.Value.path) ? new System.IO.FileInfo(syncRoot + val.Value.path).Length : 0;
                val.Value.done = val.Value.progress == 100;

                TimeSpan.TryParse(m.Groups["eta"].Value, out TimeSpan eta);
                val.Value.eta = eta != null ? eta : new TimeSpan(0, 0, 0);

                files[val.Key] = val.Value;

                lastUpdate = DateTime.Now;

                if (OnTransfer != null)
                    OnTransfer(this, new OneDriveEventArgs(val.Value));
            }
            #endregion

            #region Check if it's a transfer (Deleting/Downloading/Uploading)
            string newFilePattern = @"^(?<job>(Uploading new)|(Deleting)|(Uploading)|(Downloading)|(Creating))(\sfile\s)?(\smodified\sfile\s)?( item from OneDrive: )?( directory: )?(?<path>.*)$(?<!([0-9]{2}:[0-9]{2}:[0-9]{2}$)|(--:--:--:$))";

            foreach (Match m in Regex.Matches(line, newFilePattern, options))
            {
                // Confirm that this isn't a transfer progress
                if (!line.StartsWith("Deleting") && !System.IO.File.Exists(syncRoot + m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", "")))
                    return;

                /// Remove it if it was already there.
                if (files.ContainsKey(m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", "")))
                    files.Remove(m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", ""));

                File f = new File();

                f.path = m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", "").StartsWith("./") ? m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", "").Substring(2) : m.Groups["path"].Value.Replace(" ... done.", "").Replace(" ... ", "");
                f.job = m.Groups["job"].Value == "Downloading" ? JobType.Downloading : m.Groups["job"].Value == "Deleting" ? JobType.Deleting : JobType.Uploading;
                f.done = m.Groups["path"].Value.EndsWith(" ... done.");
                f.progress = 100;
                f.eta = new TimeSpan();
                f.size = System.IO.File.Exists(syncRoot + f.path) ? new System.IO.FileInfo(syncRoot + f.path).Length : 0;

                files.Add(f.path, f);
                
                lastUpdate = DateTime.Now;

                if (OnTransfer != null)
                    OnTransfer(this, new OneDriveEventArgs(f));
            }
            #endregion
        }

        public OneDrive(bool verbose = false, string configRoot = "")
        {
            if (!Misc.unix_simple("onedrive", "--version").StartsWith("onedrive v2.3."))
                throw new Exception("ERROR: OneDrive cli is not installed or a incompatible version is installed, expecting v2.*.\n\tInstall with: sudo apt install onedrive");

            if (configRoot == "")
                configRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/.config/onedrive";

            files = new Dictionary<string, File>();

            this.configRoot = configRoot;
            this.verbose = verbose;
            this.filesMaxSize = 100;
        }
    }
}