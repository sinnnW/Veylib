using System;
using System.Threading;
using System.Net;
using System.IO;
using System.Linq;
using System.Diagnostics;
using Octokit;

using Veylib.Utilities.Net;
using System.IO.Compression;

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
        
        /// <summary>
        /// Start the auto updater
        /// </summary>
        /// <param name="settings">Settings to use</param>
        public AutoUpdater(Settings settings)
        {
            CurrentSettings = settings;
        }

        internal void createBat(string downloadedName)
        {
            try
            {
                // Batch file.
                string content = @"@echo off
    echo Auto updater by verlox via Veylib.
    echo github.com/verlox/Veylib
    echo.
    title Waiting for application to close
    echo|set /p=""[33mWaiting for application to close[0m""

    :loop
    tasklist /FI ""PID eq {pid}"" | find /i ""{pid}"" > nul
    IF ERRORLEVEL 1 (
      goto cont
    ) else (
      timeout /T 1 > nul
      echo|set /p="".""
      goto loop
    )
    goto loop

    :cont
    echo.

    title Updating application
    echo [97mUpdating to version {version}[0m

    {drive}
    cd {dir}

    del {originalName}
    move {currentName} {originalName} > nul

    echo [92mUpdated to {version} succesfully![0m
    start {originalName}
    timeout /T 2 > nul
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
                proc.StartInfo.CreateNoWindow = CurrentSettings.HideUpdaterWindow;
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

        internal void updateFromGithub()
        {
            // Thread it.
            new Thread(() =>
            {
                try
                {
                    // Get file name and split it to parts, then add a random number
                    var split = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName).Split('.').ToList();
                    split[split.Count - 2] += $"-{rand.Next(1000, 9999)}";
                    string name = string.Join(".", split); // Concatenation

                    var client = new GitHubClient(new ProductHeaderValue("Veylib"));
                    var release = client.Repository.Release.GetAll(CurrentSettings.Github.Username, CurrentSettings.Github.Repo).GetAwaiter().GetResult().Last();

                    int fail = 0;
                    bool isZipped = false;
                    foreach (var asset in release.Assets)
                    {
                        if (asset.Name.ToLower().Contains(CurrentSettings.Github.AssetNameContains.ToLower()))
                        {
                            if (asset.Name.Contains(".zip"))
                            {
                                isZipped = true;
                                name = $"github-asset-{rand.Next(1000, 9999)}.zip";
                            }

                            // Download the file
                            NetRequest.DownloadFile(asset.BrowserDownloadUrl, name);
                            break;
                        }
                        else
                            fail++;
                    }

                    if (fail == release.Assets.Count)
                    {
                        UpdateFailed?.Invoke(this, new UpdateEventArgs { Message = "No suitable assets found " });
                        return;
                    }

                    if (isZipped)
                    {
                        fail = 0;
                        string dir = name.Remove(name.Length - 5, 4);
                        ZipFile.ExtractToDirectory(name, dir);

                        var files = Directory.GetFiles(dir);
                        foreach (var file in files)
                        {
                            if (!file.ToLower().Contains(CurrentSettings.Github.FileNameContains.ToLower()))
                                fail++;
                            else
                                name = file;
                        }

                        if (fail == files.Length)
                        {
                            UpdateFailed?.Invoke(this, new UpdateEventArgs { Message = "No suitable file found in zip archive" });
                            return;
                        }
                    }

                    // Create the batch file
                    createBat(name);
                }
                catch (Exception ex)
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

                // Events are cool
                UpdateAvailable?.Invoke(this, new UpdateEventArgs { Message = $"Update available to version {CurrentSettings.LatestVersion}" });

                // Make sure we actually update, and not just notify
                if (CurrentSettings.Mode != Settings.UpdateMode.Update)
                    return false;

                // Behind latest update, attempt update
                if (CurrentSettings.Github.Enabled)
                    updateFromGithub();
                else
                    update();
                return true;
            } catch (Exception ex)
            {
                // Bruh #2
                UpdateFailed?.Invoke(this, new UpdateEventArgs { Exception = ex });
                return false;
            }
        }

        /// <summary>
        /// Settings for the auto updater
        /// </summary>
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
            /// Hide the updater batch
            /// </summary>
            public bool HideUpdaterWindow = true;

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

            public GithubRepo Github = new GithubRepo();

            public class GithubRepo
            {
                public bool Enabled = false;
                public string Username;
                public string Repo;
                public string AssetNameContains;
                public string FileNameContains;
            }
        }
    }
}
