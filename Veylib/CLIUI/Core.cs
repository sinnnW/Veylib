using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;

// Nuget
using Newtonsoft.Json;

/*
 * Welcome to version 2 of my UI library, now
 * with an actual name. This one being more 
 * bulletproof and containing much better code.
 * The last one was a pain to work with and I'm
 * much happier with this one.
 * 
 * MADE BY VERLOX [https://verlox.cc]
 */

namespace Veylib.CLIUI
{
    public class Core
    {
        public class MessagePropertyTime
        {
            public void UpdateColor()
            {
                ColorManagement.GetInstance().HsvToRgb(Core.StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                Color = Color.FromArgb(r, g, b);
            }

            public bool Show = true;
            public string Text = DateTime.Now.ToString("HH:mm:ss.ff");
            public Color Color;
        }

        public class MessagePropertyLabel
        {
            // dictionary for printing and auto prefixing
            private readonly Dictionary<string, dynamic[]> WordToColorDict = new Dictionary<string, object[]>()
            {
                { "ok", new dynamic[] { Color.FromArgb(0, 255, 0), "  ok  " } },
                { "success", new dynamic[] { Color.FromArgb(0, 255, 0), "  ok  " } },
                { "work", new dynamic[] { Color.Yellow, " work " } },
                { "working", new dynamic[] { Color.Yellow, " work " } },
                { "fail", new dynamic[] { Color.Red, " fail " } },
                { "failure", new dynamic[] { Color.Red, " fail " } },
                { "info", new dynamic[] { Color.Cyan, " info "} },
                { "general", new dynamic[] { Color.Cyan, " info "} },
                { "none", new dynamic[] { Color.Cyan, " info "} },
                { "skip", new dynamic[] { Color.Yellow, " skip "} },
                { "conf", new dynamic[] { Color.DarkCyan, " conf " } },
                { "help", new dynamic[] { Color.DarkGreen, " help " } },
            };

            public MessagePropertyLabel()
            {
                ColorManagement.GetInstance().HsvToRgb(Core.StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                Color = Color.FromArgb(r, g, b);

                // set other values
                WordToColorDict.TryGetValue("info", out object[] val);
                Color = (Color)val[0];
                Text = val[1].ToString().ToUpper();
            }

            public void AutoFormat()
            {
                WordToColorDict.TryGetValue(Text.ToLower(), out object[] val);

                // make sure its not null
                if (val == null)
                    return;

                Color = (Color)val[0];
                Text = val[1].ToString().ToUpper();
            }

            public bool Show = true;
            public string Text;
            public Color Color;
        }

        public class MessageProperties
        {
            public void Parse(object[] MessageOrColor)
            {
                // color var and fallback
                var color = Color.FromArgb(200, 200, 200);

                foreach (var param in MessageOrColor)
                {
                    try
                    {
                        if (param == null)
                            color = Color.FromArgb(200, 200, 200);
                        else if (param is string && param.ToString().StartsWith("#") && param.ToString().Length == 7)
                        {
                            //if ((string)param == "rainbow")
                            //    color 
                            int argb = int.Parse(param.ToString().Substring(1), System.Globalization.NumberStyles.HexNumber);
                            color = Color.FromArgb(argb);
                        }
                        else if (param is Color)
                            color = (Color)param;
                        else if (param is string)
                        {
                            ColoringGroups.Add(new object[] { color, (string)param });
                            TextLength += param.ToString().Length;
                        }
                    }
                    catch { }
                }

                // format the label
                if (Label != null)
                    Label.AutoFormat();
            }

            public MessageProperties(params object[] MessageOrColor)
            {
                Parse(MessageOrColor);

                // setup the nested classes
                Time = new MessagePropertyTime();
                Label = new MessagePropertyLabel();
            }

            public List<object[]> ColoringGroups = new List<object[]>();
            public bool WordWrap = true;

            // coloring
            public bool HorizontalRainbow = false;
            public bool VerticalRainbow = false;

            public bool ShowHeaderAfter = false;
            public bool Center = false;

            public int TextLength = 0;

            public MessagePropertyTime Time;
            public MessagePropertyLabel Label;
        }

        public class StartupInterfaceProperties
        {
            public string Username = Environment.UserName;
            public Color UserColor = Color.FromArgb(3, 84, 204);

            public string Host = Environment.MachineName;
            public Color HostColor = Color.FromArgb(100, 7, 247);

            public bool ShowNextLine = false;
        }

        public class StartupAuthorProperties
        {
            public string Name;
            public string Url;
        }

        public class StartupConsoleTitleProperties
        {
            public bool Animated = false;
            public int AnimateDelay = 250;
            public string Text = Console.Title;
            public string Status;
        }

        public class StartupProperties
        {
            public StartupProperties()
            {
                Title = new StartupConsoleTitleProperties();
                Author = new StartupAuthorProperties();
                UserInformation = new StartupInterfaceProperties();
            }


            public StartupConsoleTitleProperties Title;
            public StartupAuthorProperties Author;
            public StartupInterfaceProperties UserInformation;

            public string LogoString;
            public string Version = null;
            public string MOTD;

            public bool SilentStart = false;
            public bool AutoSize = true;
            public bool UseAutoVersioning = false;
            public bool DebugMode = false;

            public int ColorRotation = 0;
            public int ColorRotationOffset = 5;
        }
        private static Core inst = null;
        public static Core GetInstance()
        {
            if (inst == null)
                inst = new Core();
            return inst;
        }

        // dll imports
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        public static Queue<MessageProperties> WriteQueue = new Queue<MessageProperties>();
        public static StartupProperties StartProperty;

        // if this is true, it will pause printing
        public static bool PauseConsole = false;
        public static bool HeaderPrintedLast = false;

        private static int cursorY = 0;
        private static int colorRotationStart = 0;
        private Thread workThread;

        // delegates
        public delegate void _noReturn();
        public delegate void _workItem(MessageProperties Message);

        // events
        public static event _noReturn OnClear;
        public static event _noReturn QueueCleared;
        public static event _workItem ItemAddedToQueue;
        public static event _workItem ItemFinished;

        public void Start(StartupProperties StartProperties)
        {
            Console.Clear();

            StartProperty = StartProperties;
            colorRotationStart = StartProperties.ColorRotation;

            // set the console mode to support colors
            var Handle = GetStdHandle(-11);
            uint Mode;
            GetConsoleMode(Handle, out Mode);
            SetConsoleMode(Handle, Mode | 0x4);

            // autoversion
            if (StartProperties.UseAutoVersioning)
                StartProperties.Version = GetAutoVersion().ToString();

            // find the longest line in the logo
            int longestLen = 0;
            if (StartProperties.LogoString != null)
                foreach (var line in StartProperties.LogoString.Split('\n'))
                    if (line.Length > longestLen)
                        longestLen = line.Length;

            // set the console size to 150 or the longest line in the logo, whichevers longer
            if (StartProperties.AutoSize)
            {
                Console.WindowWidth = (longestLen < 115 ? 115 : longestLen);
                Console.BufferWidth = Console.WindowWidth;
            }

            // print hte logo and attributions
            PrintLogo();

            // setup the title loop if enabled
            if (StartProperties.Title != null)
            {
                Console.Title = StartProperties.Title.Text;
                if (StartProperties.Title.Animated)
                {
                    if (!StartProperties.SilentStart)
                        WriteLine(new MessageProperties { Label = new MessagePropertyLabel { Text = "work" } }, "Starting animated title thread");

                    // start the animated title
                    new Thread(animatedTitleLoop).Start();

                    if (!StartProperties.SilentStart)
                        WriteLine(new MessageProperties { Label = new MessagePropertyLabel { Text = "ok" } }, "Animated title thread started");
                }
            }

            if (!StartProperties.SilentStart)
                WriteLine(new MessageProperties { Label = new MessagePropertyLabel { Text = "work" } }, "Starting work loop thread");

            // start the writeloop
            workThread = new Thread(workLoop);
            workThread.Start();

            if (!StartProperties.SilentStart)
                WriteLine(new MessageProperties { Label = new MessagePropertyLabel { Text = "ok" } }, "Work thread started");

            ItemAddedToQueue += (msg) =>
            {
                if (StartProperties.DebugMode)
                {
                    Debug.WriteLine($"New item in queue: {JsonConvert.SerializeObject(msg)}");
                }
            };
        }

        bool prevTog;
        public void Clear()
        {
            if (WriteQueue.Count > 0)
                WriteQueue.Clear();

            // Abort write thread
            if (workThread != null && workThread.IsAlive)
                workThread.Abort();

            prevTog = StartProperty.UserInformation.ShowNextLine;
            if (prevTog)
                StartProperty.UserInformation.ShowNextLine = false;

            StartProperty.ColorRotation = colorRotationStart;

            Console.Clear();
            cursorY = 0; // reset the console Y

            // print the logo
            PrintLogo();

            if (workThread != null && !workThread.IsAlive)
            {
                workThread = new Thread(workLoop);
                workThread.Start();
            }

            // trigger event
            OnClear?.Invoke();
        }

        public void PrintHeader()
        {
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            Console.Write($"\r{createColorString(StartProperty.UserInformation.UserColor)}{StartProperty.UserInformation.Username}");
            Console.ResetColor();
            Console.Write($"@{createColorString(StartProperty.UserInformation.HostColor)}{StartProperty.UserInformation.Host}");
            Console.ResetColor();
            Console.Write(" #~ ");

            HeaderPrintedLast = true;
        }

        // get the version from teh assembly file, if you use '*' in the file, it will auto increment.
        public Version GetAutoVersion()
        {
            return GetType().Assembly.GetName().Version;
        }

        private string titleStatus = "";
        public void UpdateTitleStatus(string status)
        {
            titleStatus = status;
            Console.Title = $"{StartProperty.Title.Text} | {status}";
        }

        // animated the title going in and out
        private void animatedTitleLoop()
        {
            // this is to start
            //Console.Title = "";

            StringBuilder sb = new StringBuilder();

            // this will determine what way its going
            bool mode = true;
            while (true)
            {

                // this is the switch for the mode, true = out, false = in
                if (mode)
                {
                    sb.Clear();
                    // add each char on to the string and delay
                    foreach (var c in StartProperty.Title.Text)
                    {
                        sb.Append(c);
                        Console.Title = $"{sb} | {titleStatus}";
                        Thread.Sleep(StartProperty.Title.AnimateDelay);
                    }

                    // invert the mode to go back in
                    mode = false;
                    Thread.Sleep(StartProperty.Title.AnimateDelay);
                }
                else
                {
                    // remove each char from the string and delay
                    for (var x = StartProperty.Title.Text.Length; x > 0; x--)
                    {
                        string ttl = Console.Title.Substring(0, Console.Title.Length - (titleStatus.Length + 3));

                        Console.Title = $"{ttl.Substring(0, x - 1)} | {titleStatus}";
                        Thread.Sleep(StartProperty.Title.AnimateDelay);
                    }

                    // invert mode to go back out
                    mode = true;
                }
            }
        }

        // print the logo and author attributions
        public void PrintLogo()
        {
            if (StartProperty.LogoString != null)
                foreach (var line in StartProperty.LogoString.Split('\n'))
                    WriteLine(new MessageProperties { Label = null, Time = null, VerticalRainbow = true }, line);//.Replace("\r", string.Empty));

            // attributions
            if (StartProperty.Author.Name != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Made by {StartProperty.Author.Name}");
            if (StartProperty.Author.Url != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Author URL: {StartProperty.Author.Url}");
            if (StartProperty.Version != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Version {StartProperty.Version}");
            if (StartProperty.DebugMode)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, " > Debug mode enabled");

            // extra line
            WriteLine();

            // if theres an motd, write it
            if (StartProperty.MOTD != null && StartProperty.MOTD.Length > 0)
                PrintMOTD();
            else if (prevTog)
                StartProperty.UserInformation.ShowNextLine = true;
        }

        public void PrintMOTD()
        {
            Formatting form = Formatting.GetInstance();
            string div = form.CreateDivider();
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));

            WriteLine(new MessageProperties { HorizontalRainbow = true, Label = null, Time = null, Center = true }, $"{divHalf} MOTD {divHalf}");
            WriteLine();

            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, StartProperty.MOTD);

            WriteLine();
            WriteLine(new MessageProperties { HorizontalRainbow = true, Label = null, Time = null, Center = true }, $"{divHalf} MOTD {divHalf}");

            if (StartProperty.UserInformation.ShowNextLine)
                WriteLine(new MessageProperties { ShowHeaderAfter = true });
            else
                WriteLine();
        }

        public void CreateAlert(string Title, Color Divider, params string[] Lines)
        {
            Formatting form = Formatting.GetInstance();
            string div = form.CreateDivider();
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));

            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, Divider, $"{divHalf} {Title} {divHalf}");
            WriteLine();

            foreach (var line in Lines)
                WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, line);

            WriteLine();
            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, Divider, $"{divHalf} {Title} {divHalf}");
            WriteLine();
        }

        private void ParseWrite(object[] MessageOrColor, MessageProperties Properties = null)
        {
            if (Properties == null)
                Properties = new MessageProperties(StartProperty.ColorRotation);
            if (MessageOrColor != null)
                Properties.Parse(MessageOrColor);

            WriteQueue.Enqueue(Properties);

            // invoke the event
            //lock (ItemAddedToQueue)
            //    ItemAddedToQueue?.Invoke(Properties);
        }

        public void WriteLine(MessageProperties Properties, params object[] MessageOrColor)
        {
            ParseWrite(MessageOrColor, Properties);
        }

        public void WriteLine(params object[] MessageOrColor)
        {
            ParseWrite(MessageOrColor);
        }

        public void WriteLine(MessageProperties Properties)
        {
            if (Properties.ColoringGroups != null)
                WriteQueue.Enqueue(Properties);
        }

        public void WriteLine()
        {
            WriteQueue.Enqueue(new MessageProperties { Label = new MessagePropertyLabel { Show = false }, Time = new MessagePropertyTime { Show = false }, ColoringGroups = new List<object[]> { new object[] { null } } });
        }

        private string createColorString(Color Col)
        {
            return $"\x1b[38;2;{Col.R};{Col.G};{Col.B}m";
        }

        private void workLoop()
        {
            // never stop
            while (true)
            {
                // wrap in try so nothin breaks too badly
                try
                {
                    //Debug.WriteLine($"Running loop : {WriteQueue.Count} work left");
                    // make sure theres work to do
                    if (WriteQueue.Count == 0)
                        continue;
                    if (PauseConsole)
                        continue;

                    // get and remove the properties from the list
                    MessageProperties properties = WriteQueue.Peek();

                    // make sure that its all valid
                    if (properties == null)
                    {
                        WriteQueue.Dequeue();
                        continue;
                    }

                    if (properties.ShowHeaderAfter && properties.ColoringGroups != null && properties.ColoringGroups.Count == 0)
                    {
                        PrintHeader();
                        WriteQueue.Dequeue();
                        continue;
                    }
                    else if (properties.ColoringGroups == null || properties.ColoringGroups.Count == 0)
                    {
                        if (StartProperty.DebugMode)
                            Debug.WriteLine("Dequeueing item since coloring group is null");
                        WriteQueue.Dequeue();
                        continue;
                    }
                    else if (!(properties.ColoringGroups[0][0] is Color) && (properties.ColoringGroups[0][0] is string ? properties.ColoringGroups[0][0].ToString().ToLower() != "rainbow" : true))
                    {
                        if (properties.ColoringGroups.Count == 1) // make sure that theres one coloring group and tha
                            cursorY++;

                        Console.WriteLine();
                        WriteQueue.Dequeue();
                        continue;
                    }

                    // increase the color rotation
                    StartProperty.ColorRotation += StartProperty.ColorRotationOffset;

                    // put it back to 0 if its 360 or more
                    if (StartProperty.ColorRotation >= 360)
                        StartProperty.ColorRotation = 0;

                    // check if its too long, if no check, this will cause a buffer overflow.
                    if (cursorY >= Console.BufferHeight)
                    {
                        Clear();
                        cursorY = 0;
                    }

                    // if the header was the last thing printed, bump one char down.
                    //if (HeaderPrintedLast)
                    //    cursorY++;


                    // set cursor position
                    Console.SetCursorPosition(0, cursorY);

                    // write the time
                    if (properties.Time != null && properties.Time.Show)
                    {
                        properties.Time.UpdateColor();
                        Console.ResetColor();
                        Console.Write("\r[");
                        Console.Write($"{createColorString(properties.Time.Color)}{properties.Time.Text}");
                        Console.ResetColor();
                        Console.Write("] ");
                    }
                    else
                        Console.Write("\r");

                    // write the message
                    Console.ResetColor();

                    if (properties.Center)
                    {
                        int totalLen = 0;
                        foreach (var grp in properties.ColoringGroups)
                        {
                            totalLen += grp[1].ToString().Length;
                        }
                        Console.Write(new string(' ', (int)Math.Round((decimal)(Console.BufferWidth / 2) - (totalLen / 2)) - (properties.Time != null && properties.Time.Show ? properties.Time.Text.Length + 3 : 0)));
                    }

                    if (!properties.HorizontalRainbow && !properties.VerticalRainbow)
                    {
                        foreach (var grp in properties.ColoringGroups)
                        {
                            Color clr = Color.WhiteSmoke;
                            if (grp[0].ToString() == "rainbow")
                            {
                                ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                                clr = Color.FromArgb(r, g, b);
                            }
                            if (grp[0] is Color)
                                clr = (Color)grp[0];

                            Console.Write($"{createColorString(clr)}{grp[1]}");
                            Debug.Write(grp[1]);
                        }
                    }
                    else if (properties.HorizontalRainbow)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var grp in properties.ColoringGroups)
                            sb.Append(grp[1]);

                        foreach (var c in sb.ToString())
                        {
                            ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                            Console.Write($"{createColorString(Color.FromArgb(r, g, b))}{c}");
                            StartProperty.ColorRotation++;
                        }

                        Debug.Write(sb.ToString());
                    }
                    else if (properties.VerticalRainbow)
                    {
                        StringBuilder sb = new StringBuilder();
                        foreach (var grp in properties.ColoringGroups)
                            sb.Append(grp[1]);

                        ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                        Console.Write($"{createColorString(Color.FromArgb(r, g, b))}{sb}");

                        Debug.Write(sb.ToString());
                    }

                    Debug.WriteLine("");

                    // adjust the cursors Y cood, this is used for overflowing text. it calculates this based on the length of the total line vs the buffer width
                    int total = 0; // 6 is offset of sides
                    if (properties.Label != null && properties.Label.Show)
                        total += properties.Label.Text.Length + 3;
                    if (properties.Time != null && properties.Time.Show)
                        total += properties.Time.Text.Length + 3;

                    //cursorY += (int)Math.Floor((decimal)((total + properties.TextLength) / Console.BufferWidth)) + 1;
                    cursorY++;

                    // write the label
                    if (properties.Label != null && properties.Label.Show)
                    {
                        Console.CursorLeft = Console.BufferWidth - properties.Label.Text.Length - 4;

                        Console.ResetColor();
                        Console.Write(" [");
                        Console.Write($"{createColorString(properties.Label.Color)}{properties.Label.Text}");
                        Console.ResetColor();
                        Console.Write("]");
                    }

                    HeaderPrintedLast = false;

                    // create final writing
                    Console.WriteLine();
                    Console.ResetColor();

                    if (WriteQueue.Count > 0)
                        WriteQueue.Dequeue();

                    if (WriteQueue.Count == 0)
                        QueueCleared?.Invoke();

                    if (StartProperty.UserInformation != null)
                    {
                        if (StartProperty.UserInformation.ShowNextLine && WriteQueue.Count == 0)
                            PrintHeader();
                    }

                    // evnet
                    ItemFinished?.Invoke(properties);
                }
                catch (Exception ex)
                {
                    if (WriteQueue.Count > 0)
                        WriteQueue.Dequeue();

                    // some error
                    Debug.WriteLine(ex);
                    Debug.WriteLine(new StackTrace());
                }
            }
        }

        public string ReadLine(string Pre)
        {
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            if (cursorY < 0)
                cursorY = 0;

            //Console.SetCursorPosition(6 + (StartProperty.UserInformation != null ? StartProperty.UserInformation.Username.Length + StartProperty.UserInformation.Host.Length : 0), cursorY);
            cursorY++;

            Console.Write($"\r{Pre}");
            return Console.ReadLine();
        }

        public string ReadLine()
        {
            return ReadLine("");
        }

        public string ReadLineProtected(string Pre = null)
        {
            StringBuilder sb = new StringBuilder();

            if (Pre != null)
                Console.Write(Pre);

            while (true)
            {
                var key = Console.ReadKey();

                if (key.Key == ConsoleKey.Backspace) // Ignore backspace
                {
                    if (sb.Length == 0)
                    {
                        Console.Write(" ");
                        continue;
                    }

                    sb.Remove(sb.Length - 1, 1);
                    Console.SetCursorPosition(Pre.Length + (sb.Length), cursorY);
                    Console.Write(" ");
                    Console.SetCursorPosition(Pre.Length + (sb.Length), cursorY);
                    continue;
                }
                else if (key.Key == ConsoleKey.Enter) // Return string if finished
                {
                    cursorY++;
                    return sb.ToString();
                }

                sb.Append(key.KeyChar);

                Console.SetCursorPosition(Pre.Length + (sb.Length - 1), cursorY);
                Console.Write("*");
            }
        }

        public void Delay(int ms)
        {
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            Thread.Sleep(ms);
        }
    }
}
