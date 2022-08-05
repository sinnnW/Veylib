using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Linq;
using System.Diagnostics;

using Veylib.Utilities.Net;

namespace Veylib.Utilities
{
    public class AutoUpdater
    {
        public class UpdateEventArgs : EventArgs
        {
            public string Message;
            public Exception Exception;
        }

        public static event EventHandler UpdateAvailable;
        public static event EventHandler UpdateFailed;
        public static event EventHandler RestartRequired;

        public Settings CurrentSettings = new Settings();
        private Random rand = new Random();

        public AutoUpdater()
        {

        }

        internal void createBat(string downloadedName)
        {
            try
            {
                string content = @"@echo off

    title Waiting for application to close
    echo [43mWaiting for application to close[0m

    :loop
    tasklist /FI ""PID eq {pid}"" | find /i ""{pid}""
    IF ERRORLEVEL 1 (
      GOTO cont
    ) else (
      TIMEOUT /T 1 > nul
      GOTO loop
    )
    GOTO loop

    :cont

    title Updating application
    echo [97mUpdating to version {version}[0m

    {drive}
    cd {dir}

    del {originalName}
    move {currentName} {originalName}

    echo [92mUpdated![0m
    start {originalName}
    ";

                string dir = string.Join("\\", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                string drive = $"{dir.Split(':')[0]}:";

                content = content.Replace("{version}", CurrentSettings.LatestVersion.ToString());
                content = content.Replace("{drive}", drive);
                content = content.Replace("{dir}", dir);
                content = content.Replace("{originalName}", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
                content = content.Replace("{currentName}", downloadedName);
                content = content.Replace("{pid}", Process.GetCurrentProcess().Id.ToString());

                string updaterPath = Path.Combine(Path.GetTempPath(), $"{rand.Next(1000, 9999)}-updater.bat");
                File.WriteAllText(updaterPath, content);

                var proc = new Process();
                proc.StartInfo.FileName = updaterPath;
                proc.Start();

                RestartRequired?.Invoke(this, new UpdateEventArgs { Message = "Download complete, awaiting exit." });

                if (CurrentSettings.AutoRestart)
                    Environment.Exit(0);
            } catch (Exception ex)
            {
                UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
            }
        }

        internal void update()
        {
            // Thread it.
            new Thread(() =>
            {
                try
                {
                    UpdateAvailable?.Invoke(this, new UpdateEventArgs { Message = $"Update available to version {CurrentSettings.LatestVersion}" });

                    if (CurrentSettings.Mode != Settings.UpdateMode.Update)
                        return;

                    var split = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName).Split('.').ToList();
                    split[0] += $"-{rand.Next(100, 999)}";
                    string name = string.Join(".", split);

                    NetRequest.DownloadFile(CurrentSettings.DownloadUri, name);

                    createBat(name);
                } catch (Exception ex)
                {
                    UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
                }
            }).Start();
        }

        public bool Check()
        {
            try
            {
                var resp = new NetRequest(CurrentSettings.LatestVersionUri).Send();
                if (resp.Status != HttpStatusCode.OK)
                    throw new Exception("Failed to check latest version");

                var latest = Version.Parse(resp.Content);
                CurrentSettings.LatestVersion = latest;

                var diff = latest.CompareTo(CurrentSettings.CurrentVersion);

                if (diff <= 0)
                    return false;

                update();
                return true;
            } catch (Exception ex)
            {
                UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
                return false;
            }
        }

        public class Settings
        {
            public enum UpdateMode
            {
                Notify,
                Update
            }

            public bool AutoRestart = true;
            
            public Uri LatestVersionUri;
            public Uri DownloadUri;

            public Version CurrentVersion;
            public Version LatestVersion;

            public UpdateMode Mode = UpdateMode.Update;
        }
    }
}
