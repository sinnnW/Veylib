﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace Veylib.CLIUI
{
    public class SelectionMenu
    {
        private readonly Core core = Core.GetInstance();
        private readonly int hue;
        private readonly int curY;

        public List<string> Options = new List<string>();
        public int Index = 0;
        public SelectionMenu(params string[] opts)
        {
            hue = Core.StartProperty.ColorRotation;
            curY = Core.CursorY + opts.Length + 1;
            foreach (var opt in opts)
                Options.Add(opt);
        }

        public string Activate()
        {
            Core.newItemLock = true;
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
                        core.WriteLine(props, "> ", Color.Lime, Core.Formatting.Underline, Options[Index]);

                        Core.StartProperty.ColorRotation = hue;
                        Core.CursorY = curY;
                        Core.newItemLock = false;
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
                    core.WriteLine(props, "> ", Color.White, Core.Formatting.Underline, Options[x]);
                else
                    core.WriteLine(props, "> ", Core.Formatting.Reset, Options[x]);
            }
        }
    }
}
