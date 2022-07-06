using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Veylib.ICLI
{
    public class SelectionMenu
    {
        private readonly Core core = Core.GetInstance();
        private int hue;
        private int curY;

        public List<string> Options = new List<string>();
        public Settings CurrentSettings;
        public int Index = 0;

        public class Settings
        {
            public Settings()
            {
                Style = new Style();
            }

            public Style Style;
        }

        public class Style
        {
            public Color NeutralColor = Color.WhiteSmoke;
            public Color SelectionHighlightColor = Color.White;
            public Color SelectedColor = Color.Lime;
            
            public string SelectionFormatTags = Core.Formatting.Underline;
            public string SelectedFormatTags = Core.Formatting.Underline;

            public string PreOptionText = "> ";
            public string PreOptionFormatTags = "";
            public string PreOptionHighlightFormatTags = "";
            public string PreOptionSelectedFormatTags = "";

            public Color PreOptionColor = Color.WhiteSmoke;
            public Color PreOptionHighlightColor = Color.White;
            public Color PreOptionSelectedColor = Color.WhiteSmoke;
        }

        public SelectionMenu(params string[] opts)
        {
            CurrentSettings = new Settings();
            Options.AddRange(opts);
        }

        public SelectionMenu(Settings settings)
        {
            CurrentSettings = settings;
        }

        public SelectionMenu(Settings settings, params string[] opts)
        {
            CurrentSettings = settings;
            Options.AddRange(opts);
        }

        public void AddOption(string option)
        {
            Options.Add(option);
        }

        public void RemoveOption(string option)
        {
            Options.Remove(option);
        }

        public string Activate()
        {
            hue = Core.StartProperty.ColorRotation;
            curY = Core.CursorY + Options.Count + 1;

            Core.NewItemLock = true;
            var cursorVisible = Console.CursorVisible;
            Console.CursorVisible = false;
            while (true)
            {
                Render();
                var key = Console.ReadKey();
                switch (key.Key)
                {
                    case ConsoleKey.DownArrow:
                        if (Index < Options.Count - 1)
                            Index++;
                        break;
                    case ConsoleKey.UpArrow:
                        if (Index > 0)
                            Index--;
                        break;
                    case ConsoleKey.Enter:
                        var props = new Core.MessageProperties { Label = null, Time = null, BypassLock = true, YCood = Core.CursorY - Options.Count + Index + 2 };
                        core.WriteLine(props, CurrentSettings.Style.PreOptionSelectedColor, CurrentSettings.Style.PreOptionSelectedFormatTags, CurrentSettings.Style.PreOptionText, Core.Formatting.Reset, CurrentSettings.Style.SelectedColor, CurrentSettings.Style.SelectedFormatTags, Options[Index]);

                        Core.StartProperty.ColorRotation = hue;
                        Core.CursorY = curY;
                        Core.NewItemLock = false;
                        Console.CursorVisible = cursorVisible;
                        return Options[Index];
                }
            }
        }

        public void Render()
        {
            while (Core.WriteQueue.Count > 0)
                Thread.Sleep(100);

            //renderCount++;
            for (var x = 0; x < Options.Count; x++)
            {
                var props = new Core.MessageProperties { Label = null, Time = null, BypassLock = true, YCood = Core.CursorY - Options.Count + (x + 2), NoNewLine = true };
                if (x == Index)
                    core.WriteLine(props, CurrentSettings.Style.PreOptionHighlightColor, CurrentSettings.Style.PreOptionHighlightFormatTags, CurrentSettings.Style.PreOptionText, Core.Formatting.Reset, CurrentSettings.Style.SelectionHighlightColor, CurrentSettings.Style.SelectionFormatTags, Options[x]);
                else
                    core.WriteLine(props, CurrentSettings.Style.PreOptionColor, CurrentSettings.Style.PreOptionFormatTags, CurrentSettings.Style.PreOptionText, Core.Formatting.Reset, CurrentSettings.Style.NeutralColor, Options[x]);
            }
        }
    }
}
