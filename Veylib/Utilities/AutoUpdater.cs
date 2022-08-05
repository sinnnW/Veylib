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
                // Batch file.
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

                // Get directory and drive, in case it's in another one than the temp directory
                string dir = string.Join("\\", Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                string drive = $"{dir.Split(':')[0]}:";

                // Replace all placeholders with correct information
                content = content.Replace("{version}", CurrentSettings.LatestVersion.ToString());
                content = content.Replace("{drive}", drive);
                content = content.Replace("{dir}", dir);
                content = content.Replace("{originalName}", Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName));
                content = content.Replace("{currentName}", downloadedName);
                content = content.Replace("{pid}", Process.GetCurrentProcess().Id.ToString());

                // Save updater to this path
                string updaterPath = Path.Combine(Path.GetTempPath(), $"{rand.Next(1000, 9999)}-updater.bat");
                File.WriteAllText(updaterPath, content);

                // Create process
                var proc = new Process();
                proc.StartInfo.FileName = updaterPath;
                proc.Start();

                // Application can be restarted now
                RestartRequired?.Invoke(this, new UpdateEventArgs { Message = "Download complete, awaiting exit." });

                // Exit if enabled
                if (CurrentSettings.AutoRestart)
                    Environment.Exit(0);
            } catch (Exception ex)
            {
                // AHHHHHHHHHHHHHHHHHHHHHHHHHHHHH
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
                    // Events are cool
                    UpdateAvailable?.Invoke(this, new UpdateEventArgs { Message = $"Update available to version {CurrentSettings.LatestVersion}" });

                    // Make sure we actually update, and not just notify
                    if (CurrentSettings.Mode != Settings.UpdateMode.Update)
                        return;

                    // Get file name and split it to parts, then add a random number
                    var split = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName).Split('.').ToList();
                    split[split.Count - 2] += $"-{rand.Next(100, 999)}";
                    string name = string.Join(".", split); // Concatenation

                    // Download the file
                    NetRequest.DownloadFile(CurrentSettings.DownloadUri, name);

                    // Create the batch file
                    createBat(name);
                } catch (Exception ex)
                {
                    // Bruh.
                    UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
                }
            }).Start();
        }

        /// <summary>
        /// Check for a newer update
        /// </summary>
        /// <returns>True if update required</returns>
        public bool Check()
        {
            try
            {
                // Send a request to attempt to get the latest version
                var resp = new NetRequest(CurrentSettings.LatestVersionUri).Send();
                
                // Make sure that worked
                if (resp.Status != HttpStatusCode.OK)
                    throw new Exception("Failed to check latest version");

                // Parse it into a version and save it
                var latest = Version.Parse(resp.Content);
                CurrentSettings.LatestVersion = latest;

                // See if there's any difference between the versions
                var diff = latest.CompareTo(CurrentSettings.CurrentVersion);

                // If the diff is or below 0, it means it's either ahead of the latest release shown, or it's the current release.
                if (diff <= 0)
                    return false;

                // Behind latest update, attempt update
                update();
                return true;
            } catch (Exception ex)
            {
                // Bruh #2
                UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
                return false;
            }
        }

        public class Settings
        {
            /// <summary>
            /// Update or just a notification through UpdateAvailable event
            /// </summary>
            public enum UpdateMode
            {
                Notify,
                Update
            }

            /// <summary>
            /// Automatically restart the application when ready
            /// </summary>
            public bool AutoRestart = true;
            
            /// <summary>
            /// URI of **RAW** text of version (EXAMPLE OF VALID SITE CONTENT: 1.0.0.0)
            /// </summary>
            public Uri LatestVersionUri;

            /// <summary>
            /// The URI for the file
            /// </summary>
            public Uri DownloadUri;

            /// <summary>
            /// The current version
            /// </summary>
            public Version CurrentVersion;

            /// <summary>
            /// Latest version available
            /// </summary>
            public Version LatestVersion;

            /// <summary>
            /// The current mode
            /// </summary>
            public UpdateMode Mode = UpdateMode.Update;
        }
    }
}
