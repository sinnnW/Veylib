using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

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
        public static class Formatting
        {
            public static readonly string Reset = "\x1B[0m";
            public static readonly string Underline = "\x1B[4m";
            public static readonly string Bold = "\x1b[2m";
            public static readonly string Italic = "\x1b[3m";
            public static readonly string Blink = "\x1b[5m";

            public static string CreateDivider()
            {
                return new string('=', (int)Math.Round(Console.BufferWidth / 1.25));
            }

            public static string CreateDivider(string Title)
            {
                var str = CreateDivider();
                var half = str.Substring(0, (int)Math.Round((decimal)str.Length / 2));
                return $"{half} {Title} {half}";
            }

            public static string Center(string Input)
            {
                return $"{new string(' ', (Console.BufferWidth / 2) - (Input.Length / 2))}{Input}";
            }

            public static string Space(string Input, int Length)
            {
                return $"{Input}{new string(' ', (Length - Input.Length > 0 ? Length - Input.Length : 0))}";
            }
        }

        public class MessagePropertyTime
        {
            public MessagePropertyTime Clone()
            {
                return (MessagePropertyTime)MemberwiseClone();
            }

            public void UpdateColor()
            {
                if (Color == null && StartProperty.DefaultMessageTime.Color == null)
                {
                    ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                    Color = Color.FromArgb(r, g, b);
                }
                else if (StartProperty.DefaultMessageTime.Color != null)
                    Color = StartProperty.DefaultMessageTime.Color;


            }

            public bool Show = true;
            public string Text = DateTime.Now.ToString("HH:mm:ss.ff");
            public Color Color;
        }

        public class MessagePropertyLabel
        {
            public MessagePropertyLabel Clone()
            {
                return (MessagePropertyLabel)MemberwiseClone();
            }

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
                ColorManagement.GetInstance().HsvToRgb(StartProperty?.ColorRotation ?? 0, 1, 1, out int r, out int g, out int b);
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

        public class DockOffset
        {
            public int Top = 0;
            public int Bottom = 0;
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
                Time = StartProperty.DefaultMessageTime.Clone();
                Label = StartProperty.DefaultMessageLabel.Clone();
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                ColoringGroups.ForEach(item => {
                    var allStrings = Array.FindAll(item, item2 => item2 is string);
                    sb.Append(string.Join("", allStrings));
                });

                return sb.ToString();
            }

            public List<object[]> ColoringGroups = new List<object[]>();
            public bool WordWrap = true;

            // coloring
            public bool HorizontalRainbow = false;
            public bool VerticalRainbow = false;

            public bool ShowHeaderAfter = false;
            public bool Center = false;
            public bool BypassLock = false;
            public bool NoNewLine = false;

            public int TextLength = 0;
            public int? YCood = null;

            public MessagePropertyTime Time;
            public MessagePropertyLabel Label;
            public DockOffset DockOffset = new DockOffset();
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

        public class StartupSpashScreenProperties
        {
            public StartupSpashScreenProperties()
            {
                ProgressBarSettings = new ProgressBar.Settings();
            }

            public bool AutoGenerate = false;
            public bool AutoCenter = true;
            
            public bool DisplayProgressBar = false;
            public ProgressBar.Settings ProgressBarSettings;
            
            public int DisplayTime = 5000; // MS
            public string Content;
        }

        public class StartupMOTDProperties
        {
            public string Text;
            public Color? DividerColor = null;
            public Color TextColor = Color.White;
        }

        public class StartupProperties
        {
            public StartupProperties()
            {
                Title = new StartupConsoleTitleProperties();
                Author = new StartupAuthorProperties();
                UserInformation = new StartupInterfaceProperties();
                SplashScreen = new StartupSpashScreenProperties();
                DefaultMessageLabel = new MessagePropertyLabel();
                DefaultMessageTime = new MessagePropertyTime();
                MOTD = new StartupMOTDProperties();
            }

            public StartupConsoleTitleProperties Title;
            public StartupAuthorProperties Author;
            public StartupInterfaceProperties UserInformation;
            public StartupSpashScreenProperties SplashScreen;
            public MessagePropertyLabel DefaultMessageLabel;
            public MessagePropertyTime DefaultMessageTime;
            public StartupMOTDProperties MOTD;

            public string LogoString;
            public string Version = null;

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

        public static List<MessageProperties> WriteQueue;
        public static StartupProperties StartProperty = null;

        // if this is true, it will pause printing
        public static bool PauseConsole = false;
        public static bool HeaderPrintedLast = false;
        public static bool newItemLock = false;

        //public static int DeadspaceTop = 0;
        public static int DeadspaceBottom = 0;

        public static int CursorY = 0;
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

        public void Start(StartupProperties startProperties = null)
        {
            Console.Clear();
            Debug.WriteLine("STARTING...");

            StartProperty = startProperties == null ? new StartupProperties() : startProperties;
            WriteQueue = new List<MessageProperties>();
            colorRotationStart = StartProperty.ColorRotation;

            // set the console mode to support colors
            var Handle = GetStdHandle(-11);
            uint Mode;
            GetConsoleMode(Handle, out Mode);
            SetConsoleMode(Handle, Mode | 0x4);

            // autoversion
            if (StartProperty.UseAutoVersioning)
                StartProperty.Version = GetAutoVersion().ToString();

            // find the longest line in the logo
            int longestLen = 0;
            if (StartProperty.LogoString != null)
                foreach (var line in StartProperty.LogoString.Split('\n'))
                    if (line.Length > longestLen)
                        longestLen = line.Length;

            // set the console size to 150 or the longest line in the logo, whichevers longer
            if (StartProperty.AutoSize)
            {
                Console.WindowWidth = (longestLen < 115 ? 115 : longestLen);
                Console.BufferWidth = Console.WindowWidth;
            }

            // setup the title loop if enabled
            if (StartProperty.Title != null)
            {
                Console.Title = StartProperty.Title.Text;
                if (StartProperty.Title.Animated)
                    new Thread(animatedTitleLoop).Start();
            }

            // start the writeloop
            workThread = new Thread(workLoop);
            workThread.Start();

            // show splash if enabled
            ShowSplash();

            // print the logo and attributions
            PrintLogo();

            if (StartProperty.DebugMode)
            {
                ItemAddedToQueue += (msg) =>
                {
                    Debug.WriteLine($"New item in queue: {JsonConvert.SerializeObject(msg)}");
                };
            }
        }

        bool prevTog;
        public void Clear(bool showLogo = true)
        {
            if (WriteQueue.Count > 0)
                WriteQueue.Clear();

            // Abort write thread
            //if (workThread != null && workThread.IsAlive)
            //    workThread.Abort();

            prevTog = StartProperty.UserInformation.ShowNextLine;
            if (prevTog)
                StartProperty.UserInformation.ShowNextLine = false;

            StartProperty.ColorRotation = colorRotationStart;

            Console.Clear();
            CursorY = 0; // reset the console Y

            // print the logo
            if (showLogo)
                PrintLogo();

            //if (workThread != null && !workThread.IsAlive)
            //{
            //    workThread = new Thread(workLoop);
            //    workThread.Start();
            //}

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
                    WriteLine(new MessageProperties { Label = null, Time = null, VerticalRainbow = true }, line);

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
            if (StartProperty.MOTD != null && StartProperty.MOTD.Text.Length > 0)
                PrintMOTD();
            else if (prevTog)
                StartProperty.UserInformation.ShowNextLine = true;
        }

        public void PrintMOTD()
        {
            string div = Formatting.CreateDivider();
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));
            string divColor = StartProperty.MOTD.DividerColor == null ? "" : createColorString(StartProperty.MOTD.DividerColor ?? Color.White);
            Debug.WriteLine(divColor);

            bool rb = false;
            if (StartProperty.MOTD.DividerColor == null)
                rb = true;

            WriteLine(new MessageProperties { HorizontalRainbow = rb, Label = null, Time = null, Center = true }, $"{divColor}{divHalf} MOTD {divHalf}");
            WriteLine();

            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, StartProperty.MOTD.TextColor, StartProperty.MOTD.Text);

            WriteLine();
            WriteLine(new MessageProperties { HorizontalRainbow = rb, Label = null, Time = null, Center = true }, $"{divColor}{divHalf} MOTD {divHalf}");

            if (StartProperty.UserInformation.ShowNextLine)
                WriteLine(new MessageProperties { ShowHeaderAfter = true });
            else
                WriteLine();
        }

        public void ShowSplash()
        {
            if (StartProperty.SplashScreen == null)
                return;

            var paused = PauseConsole;
            PauseConsole = true;

            var visible = Console.CursorVisible;
            Console.CursorVisible = false;

            Clear();

            var centerAmnt = 0;
            foreach (var line in StartProperty.LogoString.Split('\n'))
            {
                var tmp = (Console.WindowWidth / 2) - (line.Length / 2);
                if (centerAmnt < tmp)
                    centerAmnt = tmp;
            }

            if (StartProperty.SplashScreen.AutoGenerate)
            {
                WriteLine(new MessageProperties { Label = null, Time = null, BypassLock = true }, new string('\n', (Console.WindowHeight / 2) - (StartProperty.LogoString.Split('\n').Length / 2) - (StartProperty.Author != null ? 1 : 0)));

                foreach (var line in StartProperty.LogoString.Split('\n'))
                    WriteLine(new MessageProperties { Label = null, Time = null, VerticalRainbow = true, BypassLock = true }, $"{new string(' ', centerAmnt)}{line}");

                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true, Center = true, BypassLock = true }, $"Made by {StartProperty.Author.Name}");
            }

            long timeStarted = General.EpochTimeMilliseconds;
            long timeEnd = timeStarted + StartProperty.SplashScreen.DisplayTime;
            if (StartProperty.SplashScreen.DisplayProgressBar)
            {
                new Thread(() => {
                    StartProperty.SplashScreen.ProgressBarSettings.TotalParts = 100;
                    var pb = new ProgressBar(StartProperty.SplashScreen.ProgressBarSettings);

                    //while ((timeStarted - General.EpochTimeMilliseconds) < timeEnd)
                    while (((int)(General.EpochTimeMilliseconds - timeStarted)) * 100 / StartProperty.SplashScreen.DisplayTime < 100)
                    {
                        pb.SetProgress(((int)(General.EpochTimeMilliseconds - timeStarted)) * 100 / StartProperty.SplashScreen.DisplayTime);
                        pb.Render();
                        Thread.Sleep(50);
                    }

                    pb.Remove();
                }).Start();
            }
            
            // Anti scroll
            //new Thread(() =>
            //{
            //    while (StartProperty.SplashScreen.DisplayTime + timeStarted < timeEnd)
            //        Console.SetWindowPosition(0,0);
            //}).Start();

            Thread.Sleep(StartProperty.SplashScreen.DisplayTime);
            Clear(false);

            PauseConsole = paused;
            Console.CursorVisible = visible;
        }

        public void CreateAlert(string Title, Color Divider, params string[] Lines)
        {
            string div = Formatting.CreateDivider();
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));

            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, Divider, $"{divHalf} {Title} {divHalf}");
            WriteLine();

            foreach (var line in Lines)
                WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, line);

            WriteLine();
            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, Divider, $"{divHalf} {Title} {divHalf}");
            WriteLine();
        }

        private void parseWrite(object[] messageOrColor, MessageProperties properties = null)
        {
            if (StartProperty == null)
                Start();

            if (properties == null)
                properties = new MessageProperties(StartProperty.ColorRotation);
            if (messageOrColor != null)
                properties.Parse(messageOrColor);

            while (newItemLock && !properties.BypassLock)
                Thread.Sleep(100);

            WriteQueue.Add(properties);

            // invoke the event
            if (ItemAddedToQueue != null)
                lock (ItemAddedToQueue)
                    ItemAddedToQueue?.Invoke(properties);
        }

        public void WriteLine(MessageProperties properties, params object[] messageOrColor)
        {
            parseWrite(messageOrColor, properties);
        }

        public void WriteLine(params object[] messageOrColor)
        {
            parseWrite(messageOrColor);
        }

        public void WriteLine(MessageProperties properties)
        {
            while (newItemLock && !properties.BypassLock)
                Thread.Sleep(100);

            if (properties.ColoringGroups != null)
                WriteQueue.Add(properties);
        }

        public void WriteLine()
        {
            while (newItemLock)
                Thread.Sleep(100);

            WriteQueue.Add(new MessageProperties { Label = new MessagePropertyLabel { Show = false }, Time = new MessagePropertyTime { Show = false }, ColoringGroups = new List<object[]> { new object[] { null } } });
        }

        Regex colorStringRegex = new Regex(@"(\x1b)[\[\]0-9;]{0,99}m");
        private string createColorString(Color Col)
        {
            return $"\x1b[38;2;{Col.R};{Col.G};{Col.B}m";
        }

        static int prevY = 0;
        internal static void setWindow()
        {
            // fucking aids
            int y = CursorY - (Console.WindowHeight - DeadspaceBottom);
            if (DeadspaceBottom > 0 && y > 0)
                Console.SetWindowPosition(0, y);
            else if (DeadspaceBottom == 0)
                prevY = Console.WindowTop;
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

                    setWindow();

                    // get and remove the properties from the list
                    int index = 0;
                    MessageProperties properties = WriteQueue[index];

                    if (PauseConsole)
                    {
                        var filtered = WriteQueue.FindAll(item => item != null && item.BypassLock);

                        if (filtered.Count > 0)
                        {
                            properties = filtered[0];
                            index = WriteQueue.IndexOf(filtered[0]);
                        }
                        else
                            continue; 
                    }

                    // make sure that its all valid
                    if (properties == null)
                    {
                        WriteQueue.RemoveAt(index);
                        continue;
                    }
                    else if (properties.ShowHeaderAfter && properties.ColoringGroups != null && properties.ColoringGroups.Count == 0)
                    {
                        PrintHeader();
                        WriteQueue.RemoveAt(index);
                        continue;
                    }
                    else if (properties.ColoringGroups == null || properties.ColoringGroups.Count == 0)
                    {
                        if (StartProperty.DebugMode)
                            Debug.WriteLine("Dequeueing item since coloring group is null");
                        WriteQueue.RemoveAt(index);
                        continue;
                    }
                    else if (!(properties.ColoringGroups[0][0] is Color) && (properties.ColoringGroups[0][0] is string ? properties.ColoringGroups[0][0].ToString().ToLower() != "rainbow" : true))
                    {
                        if (properties.ColoringGroups.Count == 1 && !properties.NoNewLine) // make sure that theres one coloring group and tha
                            CursorY++;

                        Console.WriteLine();
                        WriteQueue.RemoveAt(index);
                        continue;
                    }

                    // increase the color rotation
                    StartProperty.ColorRotation += StartProperty.ColorRotationOffset;

                    // put it back to 0 if its 360 or more
                    if (StartProperty.ColorRotation >= 360)
                        StartProperty.ColorRotation = 0;

                    // check if its too long, if no check, this will cause a buffer overflow.
                    if (CursorY >= Console.BufferHeight)
                    {
                        Clear();
                        CursorY = 0;
                    }

                    // if the header was the last thing printed, bump one char down.
                    //if (HeaderPrintedLast)
                    //    CursorY++;


                    // set cursor position
                    if (properties.DockOffset.Top > 0)
                        Console.SetCursorPosition(0, Console.WindowTop + properties.DockOffset.Top);
                    else if (properties.DockOffset.Bottom > 0)
                        Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - properties.DockOffset.Bottom);
                    else if (properties.YCood == null)
                        Console.SetCursorPosition(0, CursorY);
                    else
                        Console.SetCursorPosition(0, properties.YCood ?? 0);

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
                            foreach (Match match in colorStringRegex.Matches(grp[1].ToString().ToLower()))
                            {
                                Debug.WriteLine("matlen" + match.Length);
                                totalLen -= match.Length;
                            }

                            totalLen += grp[1].ToString().Length;
                        }
                        int cnt = (int)Math.Round((decimal)(Console.BufferWidth / 2) - (totalLen / 2)) - (properties.Time != null && properties.Time.Show ? properties.Time.Text.Length + 3 : 0);
                        Console.Write(new string(' ', cnt < 0 ? 0 : cnt));
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

                    // adjust the cursors Y cood, this is used for overflowing text. it calculates this based on the length of the total line vs the buffer width
                    int total = 0; // 6 is offset of sides
                    if (properties.Label != null && properties.Label.Show)
                        total += properties.Label.Text.Length + 3;
                    if (properties.Time != null && properties.Time.Show)
                        total += properties.Time.Text.Length + 3;

                    //CursorY += (int)Math.Floor((decimal)((total + properties.TextLength) / Console.BufferWidth)) + 1;
                    if (!properties.NoNewLine)
                        CursorY += properties.ToString().Split('\n').Length;

                    // write the label
                    if (properties.Label != null && properties.Label.Show)
                    {
                        Console.Write(new string(' ', Console.BufferWidth - properties.Label.Text.Length - 4 - Console.CursorLeft));
                        Console.CursorLeft = Console.BufferWidth - properties.Label.Text.Length - 4;

                        Console.ResetColor();
                        Console.Write(" [");
                        Console.Write($"{createColorString(properties.Label.Color)}{properties.Label.Text}");
                        Console.ResetColor();
                        Console.Write("]");
                    }

                    HeaderPrintedLast = false;

                    // create final writing
                    if (!properties.NoNewLine)
                    {
                        Console.WriteLine();
                        Debug.WriteLine("");
                    }
                    Console.ResetColor();

                    if (WriteQueue.Count > 0)
                        WriteQueue.RemoveAt(index);

                    if (WriteQueue.Count == 0)
                        QueueCleared?.Invoke();

                    if (StartProperty.UserInformation != null)
                    {
                        if (StartProperty.UserInformation.ShowNextLine && WriteQueue.Count == 0)
                            PrintHeader();
                    }

                    setWindow();

                    // evnet
                    ItemFinished?.Invoke(properties);
                }
                catch (Exception ex)
                {
                    if (WriteQueue.Count > 0)
                        WriteQueue.RemoveAt(0);

                    // some error
                    Debug.WriteLine(ex);
                    Debug.WriteLine(new StackTrace());
                }
            }
        }

        public string ReadLine(string pre = "", Color? inputColor = null, int startingPos = 0)
        {
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            if (CursorY < 0)
                CursorY = 0;

            Console.SetCursorPosition(startingPos, CursorY);

            CursorY++;
            Console.Write(pre);

            if (inputColor != null)
                Console.Write(createColorString(inputColor ?? Color.White));

            return Console.ReadLine();
        }

        public string ReadLineProtected(string pre = null, Color? inputColor = null, int startingPos = 0)
        {
            StringBuilder sb = new StringBuilder();

            if (pre != null)
                Console.Write(pre);

            if (inputColor != null)
                Console.Write(createColorString(inputColor ?? Color.White));

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
                    Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length), CursorY);
                    Console.Write(" ");
                    Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length), CursorY);
                    continue;
                }
                else if (key.Key == ConsoleKey.Enter) // Return string if finished
                {
                    CursorY++;
                    return sb.ToString();
                }

                sb.Append(key.KeyChar);

                Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length - 1), CursorY);
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
