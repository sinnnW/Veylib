// Sys
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

// TODO: Coloring.

namespace Veylib.ICLI
{
    /// <summary>
    /// A customizable progress bar for console applications
    /// </summary>
    public class ProgressBar
    {
        private int lastLineY = 0;
        private bool removed = false;
        public ProgressBar(Settings settings)
        {
            CurrentSettings = settings;
        }

        public ProgressBar(int totalParts)
        {
            CurrentSettings = new Settings { TotalParts = totalParts };
        }

        public ProgressBar(double totalParts)
        {
            CurrentSettings = new Settings { TotalParts = totalParts };
        }

        public ProgressBar()
        {
            CurrentSettings = new Settings { };
        }

        // Probably gonna need this
        private Core core = Core.GetInstance();

        /// <summary>
        /// Docking positions for the progress bar
        /// </summary>
        public enum Dock
        {
            Top,
            Center,
            Bottom
        }

        /// <summary>
        /// Settings for the progress bar
        /// </summary>
        public class Settings
        {
            /// <summary>
            /// Total parts of the bar
            /// </summary>
            public double TotalParts = 4;

            /// <summary>
            /// Current progression
            /// </summary>
            public double Progress = 0;

            /// <summary>
            /// Styling
            /// </summary>
            public Style Style = new Style();
        }

        /// <summary>
        /// Custom styling
        /// </summary>
        public class Style
        {
            /// <summary>
            /// What character should each part of the progress bar be
            /// </summary>
            public char FillingChar = '=';

            /// <summary>
            /// The color of the characters that fill the bar
            /// </summary>
            public Color FillingColor = Color.Gray;

            /// <summary>
            /// The bracket color
            /// </summary>
            public Color EdgeColor = Color.White;

            /// <summary>
            /// The completion label color
            /// </summary>
            public Color CompletionLabelColor = Color.White;

            /// <summary>
            /// Completion label (ex: 10/20)
            /// </summary>
            public bool DisplayCompletion = false;

            /// <summary>
            /// Margin size
            /// </summary>
            public int SideSpace = 5;

            /// <summary>
            /// Dock position
            /// </summary>
            public Dock Dock = Dock.Bottom;
            
            /// <summary>
            /// Margin on dock
            /// </summary>
            public int DockOffset = 1;
        }

        /// <summary>
        /// Current settings
        /// </summary>
        public Settings CurrentSettings;

        /// <summary>
        /// Get the size of each part
        /// </summary>
        public double PartLength
        {
            get { return (Console.WindowWidth - 2 - (CurrentSettings.Style.SideSpace * 2)) / CurrentSettings.TotalParts; }
        }

        /// <summary>
        /// Get the size of each part (when completion label is enabled)
        /// </summary>
        public double  PartLengthWithCompletion
        {
            get { return (Console.WindowWidth - 6 - ((CurrentSettings.TotalParts.ToString().Length * 2) + (CurrentSettings.Style.SideSpace * 2))) / CurrentSettings.TotalParts; }
        }

        /// <summary>
        /// The size of the inside of the bar
        /// </summary>
        public int InnerLength
        {
            get { return Console.WindowWidth - (CurrentSettings.Style.SideSpace * 2) - (CurrentSettings.Style.DisplayCompletion ? (CurrentSettings.TotalParts.ToString().Length * 2) + 4 : 0) - 2; }
        }

        /// <summary>
        /// Add a certain amount of progress to the completed amount
        /// </summary>
        /// <param name="progress">Amount</param>
        /// <returns>Current progress</returns>
        public double AddProgress(int progress)
        {
            // Make sure it won't overflow
            if (CurrentSettings.Progress < CurrentSettings.TotalParts)
                CurrentSettings.Progress += progress;

            // :)
            return CurrentSettings.Progress;
        }

        /// <summary>
        /// Add a single point of progress
        /// </summary>
        /// <returns>Current progress</returns>
        public double AddProgress()
        {
            return AddProgress(1);
        }

        /// <summary>
        /// Set the completed amount
        /// </summary>
        /// <param name="progress">Current progress</param>
        public void SetProgress(int progress)
        {
            // Don't allow it to go over
            if (progress > CurrentSettings.TotalParts || progress < 0)
                return;

            CurrentSettings.Progress = progress;
        }

        /// <summary>
        /// String version of the progress bar
        /// </summary>
        /// <returns>Progress bar</returns>
        public override string ToString()
        {
            // Amount of fill characters to show
            int filledLen = (int)Math.Round(CurrentSettings.Progress * (CurrentSettings.Style.DisplayCompletion ? PartLengthWithCompletion : PartLength));

            // The inside of the bar
            string inner = $"{new string(CurrentSettings.Style.FillingChar, filledLen)}{new string(' ', InnerLength - filledLen)}";

            // The completion label
            string completionInner = $"{CurrentSettings.Progress}{new string(' ', CurrentSettings.TotalParts.ToString().Length - CurrentSettings.Progress.ToString().Length)}/{CurrentSettings.TotalParts}";

            // Return the finished product
            if (CurrentSettings.Style.DisplayCompletion)
                return $"{new string(' ', CurrentSettings.Style.SideSpace)}[{inner}] [{completionInner}]";
            else
                return $"{new string(' ', CurrentSettings.Style.SideSpace)}[{inner}]";
        }

        private string last = string.Empty;
        /// <summary>
        /// Write onto screen
        /// </summary>
        public void Render()
        {
            // Make sure we ain't just wastin memory
            string pb = ToString();
            if (pb == last)
                return;
            else if (removed)
                return;

            // Write one line above the current line (prevents trailing progress bars)
            core.WriteLine(new Core.MessageProperties { Label = null, Time = null, NoNewLine = true, DockOffset = new Core.DockOffset { Bottom = 3 }, BypassLock = true }, new string(' ', Console.WindowWidth));

            // Get the last line in the window
            lastLineY = Console.WindowTop + Console.WindowHeight - CurrentSettings.Style.DockOffset;

            // The actual properties for the message
            var mp = new Core.MessageProperties
            {
                BypassLock = true,
                ColoringGroups = new List<object[]> { new object[] { CurrentSettings.Style.EdgeColor, $"{new string(' ', CurrentSettings.Style.SideSpace)}[" }, new object[] { CurrentSettings.Style.FillingColor, pb.Substring(CurrentSettings.Style.SideSpace + 1, InnerLength) }, new object[] { CurrentSettings.Style.EdgeColor, "]" } },
                Label = null,
                Time = null,
                DockOffset = new Core.DockOffset { Bottom = 2 },
                NoNewLine = true
            };

            // Adding final groups
            if (CurrentSettings.Style.DisplayCompletion)
            {
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.EdgeColor, " [" });
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.FillingColor, $"{CurrentSettings.Progress}{new string(' ', CurrentSettings.TotalParts.ToString().Length - CurrentSettings.Progress.ToString().Length)}/{CurrentSettings.TotalParts}" });
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.EdgeColor, "]" });
            }

            last = pb;

            // :)
            core.WriteLine(mp);
        }

        /// <summary>
        /// Remove a progress bar
        /// </summary>
        public void Remove() 
        {
            // Save our memory or smth
            removed = true;

            // ;(
            core.WriteLine(new Core.MessageProperties { Label = null, Time = null, NoNewLine = true, YCood = lastLineY, BypassLock = true }, new string(' ', Console.WindowWidth));
        }
    }
}
