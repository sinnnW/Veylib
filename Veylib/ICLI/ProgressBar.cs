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

        public ProgressBar()
        {
            CurrentSettings = new Settings { };
        }

        public Core core = Core.GetInstance();

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
            public int TotalParts = 4;
            public int Progress = 0;
            public Style Style = new Style();
        }

        /// <summary>
        /// Custom styling
        /// </summary>
        public class Style
        {
            public char FillingChar = '=';

            public Color FillingColor = Color.Gray;
            public Color EdgeColor = Color.White;
            public Color CompletionLabelColor = Color.White;

            public bool DisplayCompletion = false;
            public int SideSpace = 5;
            public Dock Dock = Dock.Bottom;
            public int DockOffset = 1;
        }

        public Settings CurrentSettings;
        public decimal PartLength
        {
            get { return (decimal)(Console.WindowWidth - 2 - (CurrentSettings.Style.SideSpace * 2)) / CurrentSettings.TotalParts; }
        }

        public decimal PartLengthWithCompletion
        {
            get { return (decimal)(Console.WindowWidth - 6 - ((CurrentSettings.TotalParts.ToString().Length * 2) + (CurrentSettings.Style.SideSpace * 2))) / CurrentSettings.TotalParts; }
        }

        public int InnerLength
        {
            get { return Console.WindowWidth - (CurrentSettings.Style.SideSpace * 2) - (CurrentSettings.Style.DisplayCompletion ? (CurrentSettings.TotalParts.ToString().Length * 2) + 4 : 0) - 2; }
        }

        /// <summary>
        /// Add a certain amount of progress to the completed amount
        /// </summary>
        /// <param name="progress">Amount</param>
        /// <returns>Current progress</returns>
        public int AddProgress(int progress)
        {
            if (CurrentSettings.Progress < CurrentSettings.TotalParts)
                CurrentSettings.Progress += progress;

            return CurrentSettings.Progress;
        }

        /// <summary>
        /// Add a single point of progress
        /// </summary>
        /// <returns>Current progress</returns>
        public int AddProgress()
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
            int filledLen = (int)Math.Round(CurrentSettings.Progress * (CurrentSettings.Style.DisplayCompletion ? PartLengthWithCompletion : PartLength));
            //string inner = $"{new string('=', filledLen)}{new string(' ', (int)Math.Round((CurrentSettings.Style.DisplayCompletion ? PartLengthWithCompletion : PartLength) * (CurrentSettings.TotalParts - CurrentSettings.Progress)))}";
            string inner = $"{new string(CurrentSettings.Style.FillingChar, filledLen)}{new string(' ', InnerLength - filledLen)}";

            string completionInner = $"{CurrentSettings.Progress}{new string(' ', CurrentSettings.TotalParts.ToString().Length - CurrentSettings.Progress.ToString().Length)}/{CurrentSettings.TotalParts}";

            if (CurrentSettings.Style.DisplayCompletion)
            {
                return $"{new string(' ', CurrentSettings.Style.SideSpace)}[{inner}] [{completionInner}]";
            }
            else
                return $"{new string(' ', CurrentSettings.Style.SideSpace)}[{inner}]";
        }

        private string last = string.Empty;
        /// <summary>
        /// Write onto screen
        /// </summary>
        public void Render()
        {
            string pb = ToString();
            if (pb == last)
                return;
            else if (removed)
                return;

            int y = CurrentSettings.Style.DockOffset;
            switch (CurrentSettings.Style.Dock)
            {
                case Dock.Center:
                    y = (int)Math.Floor((decimal)Console.WindowHeight / 2);
                    break;
                case Dock.Bottom:
                    y = Console.WindowHeight - CurrentSettings.Style.DockOffset;
                    break;
            }

            core.WriteLine(new Core.MessageProperties { Label = null, Time = null, NoNewLine = true, DockOffset = new Core.DockOffset { Bottom = 3 }, BypassLock = true }, new string(' ', Console.WindowWidth));

            lastLineY = Console.WindowTop + Console.WindowHeight - CurrentSettings.Style.DockOffset;
            var mp = new Core.MessageProperties
            {
                BypassLock = true,
                ColoringGroups = new List<object[]> { new object[] { CurrentSettings.Style.EdgeColor, $"{new string(' ', CurrentSettings.Style.SideSpace)}[" }, new object[] { CurrentSettings.Style.FillingColor, pb.Substring(CurrentSettings.Style.SideSpace + 1, InnerLength) }, new object[] { CurrentSettings.Style.EdgeColor, "]" } },
                Label = null,
                Time = null,
                DockOffset = new Core.DockOffset { Bottom = 2 },
                NoNewLine = true
            };

            if (CurrentSettings.Style.DisplayCompletion)
            {
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.EdgeColor, " [" });
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.FillingColor, $"{CurrentSettings.Progress}{new string(' ', CurrentSettings.TotalParts.ToString().Length - CurrentSettings.Progress.ToString().Length)}/{CurrentSettings.TotalParts}" });
                mp.ColoringGroups.Add(new object[] { CurrentSettings.Style.EdgeColor, "]" });
            }

            last = pb;
            //Core.setWindow();
            core.WriteLine(mp);
        }

        public void Remove() 
        {
            removed = true;
            core.WriteLine(new Core.MessageProperties { Label = null, Time = null, NoNewLine = true, YCood = lastLineY, BypassLock = true }, new string(' ', Console.WindowWidth));
        }
    }
}
