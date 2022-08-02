using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;
using Microsoft.Win32;

namespace Veylib.Security
{
    public class ProcessManagement
    {
        /// <summary>
        /// The current settings
        /// </summary>
        public Settings CurrentSettings = new Settings();

        /// <summary>
        /// Violation event handler
        /// </summary>
        /// <param name="violation">Violation information</param>
        public delegate void ProcessEventHandler(Violation violation);

        /// <summary>
        /// Event for when there is a violation
        /// </summary>
        public event ProcessEventHandler ProcessViolation;

        /// <summary>
        /// Settings for process management
        /// </summary>
        public class Settings
        {
            /// <summary>
            /// All modules loaded
            /// </summary>
            internal ProcessModuleCollection LoadedModules;
            
            /// <summary>
            /// Blacklisted executables
            /// </summary>
            public List<string> BlacklistedProcessExecutables = new List<string>();

            /// <summary>
            /// Blacklisted names
            /// </summary>
            public List<string> BlacklistedProcessNames = new List<string>();

            /// <summary>
            /// Blacklisted window titles
            /// </summary>
            public List<string> BlacklistedProcessWindowTitles = new List<string>();

            /// <summary>
            /// Strict mode enabled = close on process violation
            /// </summary>
            public bool Strict = true;

            /// <summary>
            /// Interval between checks
            /// </summary>
            public TimeSpan Interval = new TimeSpan(0, 0, 15);
        }

        /// <summary>
        /// Structure for when a violation has occured
        /// </summary>
        public struct Violation
        {
            /// <summary>
            /// The associated process
            /// </summary>
            public Process Associated;

            /// <summary>
            /// Description of the violation
            /// </summary>
            public string Description;
        }

        /// <summary>
        /// Start monitors
        /// </summary>
        public void Start()
        {
            CurrentSettings.LoadedModules = Process.GetCurrentProcess().Modules;

            // Check all DLLs
            validateDlls();

            Thread.Sleep(2500);

            // Start threads for each monitor
            new Thread(processMon).Start();
            new Thread(moduleMon).Start();
        }

        /// <summary>
        /// Redo the cached checksums in the registry
        /// </summary>
        public void ReCacheModuleChecksums()
        {
            // New dictionary to supply
            var dict = new Dictionary<string, string>();

            // Iterate through each module
            foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
            {
                // Get filename
                var split = mod.FileName.Split('\\');
                string filename = split.GetValue(split.Length - 1).ToString();

                // Add to dictionary
                dict.Add(filename, Hashing.FileChecksum(mod.FileName));
            }

            // Finally, cache
            Hashing.CacheChecksums(dict);
        }

        /// <summary>
        /// There was a process that violated something
        /// </summary>
        /// <param name="violator">Process</param>
        internal void violation(Violation violation)
        {
            // Invoke violation event
            ProcessViolation?.Invoke(violation);

            // Close if strict mode is enabled
            if (CurrentSettings.Strict)
                Environment.Exit(0);
        }

        /// <summary>
        /// Validate all DLLs and make sure they all match checksums
        /// </summary>
        internal void validateDlls()
        {
            // Open subkey
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Veylib\Checksums", true);

            // Make sure it's not null, if so, create one
            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Veylib\Checksums", true);

            // Iterate through each loaded module
            foreach (ProcessModule mod in CurrentSettings.LoadedModules)
            {
                // Get checksum
                string checksum = Hashing.FileChecksum(mod.FileName);

                // Get filename from path
                var split = mod.FileName.Split('\\');
                string filename = split.GetValue(split.Length - 1).ToString();

                // Get value
                var val = key.GetValue(filename);

                // No value, store it.
                if (val == null)
                    key.SetValue(filename, checksum);
                else if (val.ToString() != checksum) // Make sure it matches, else, it's a violation
                    violation(new Violation { Description = "Module checksum invalid", Associated = new Process { StartInfo = new ProcessStartInfo { FileName = filename } } });
            }
        }

        /// <summary>
        /// Monitor system processes
        /// </summary>
        internal protected void processMon()
        {
            // No stop
            while (true)
            {
                // Make sure that it won't crash
                try
                {
                    // Gather all processes
                    var procs = Process.GetProcesses();

                    // Check each proc
                    foreach (var proc in procs)
                    {
                        // Check and see if it has a blacklisted name
                        foreach (var name in CurrentSettings.BlacklistedProcessNames)
                            if (proc.ProcessName.Contains(name))
                                violation(new Violation { Associated = proc, Description = "Blacklisted process name was detected" });

                        // Check and see if it has a blacklisted exe name
                        foreach (var exec in CurrentSettings.BlacklistedProcessExecutables)
                            if (proc.StartInfo.FileName.Contains(exec))
                                violation(new Violation { Associated = proc, Description = "Blacklisted executable was detected" });

                        // Check and see if it has a blacklisted window title
                        foreach (var title in CurrentSettings.BlacklistedProcessWindowTitles)
                            if (proc.MainWindowTitle.Contains(title))
                                violation(new Violation { Associated = proc, Description = "Blacklisted window title was detected" });
                    }
                } catch { }

                // Delay for the set interval
                Thread.Sleep(CurrentSettings.Interval);
            }
        }

        /// <summary>
        /// Monitor loaded modules
        /// </summary>
        internal void moduleMon()
        {
            // No stop
            while (true)
            {
                // No crash
                try
                {
                    // Cycle through loaded modules
                    foreach (ProcessModule mod in Process.GetCurrentProcess().Modules)
                    {
                        int fail = 0;
                        foreach (ProcessModule cachedMod in CurrentSettings.LoadedModules)
                            if (cachedMod.ModuleName != mod.ModuleName) // Make sure module is whitelisted
                                fail++;

                        if (fail >= CurrentSettings.LoadedModules.Count)
                                    violation(new Violation { Description = $"Module mismatch", Associated = new Process { StartInfo = new ProcessStartInfo { FileName = mod.FileName } } });
                    }
                }
                catch { }

                // Delay for set interval
                Thread.Sleep(CurrentSettings.Interval);
            }
        }
    }
}
