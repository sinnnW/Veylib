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

namespace Veylib.ICLI
{
    public class Core
    {
        /// <summary>
        /// This is all formatting tags + methods
        /// </summary>
        public static class Formatting
        {
            /// <summary>
            /// Reset all formatting
            /// </summary>
            public static readonly string Reset = "\x1B[0m";

            /// <summary>
            /// Underline following text
            /// </summary>
            public static readonly string Underline = "\x1B[4m";

            /// <summary>
            /// Bold following text
            /// </summary>
            public static readonly string Bold = "\x1b[2m";

            /// <summary>
            /// Italicize following text
            /// </summary>
            public static readonly string Italic = "\x1b[3m";

            /// <summary>
            /// Make following text blink
            /// </summary>
            public static readonly string Blink = "\x1b[5m";

            /// <summary>
            /// Create a color tag from a color
            /// </summary>
            /// <param name="color">Color</param>
            /// <returns></returns>
            public static string CreateColorString(Color color)
            {
                return $"\x1b[38;2;{color.R};{color.G};{color.B}m";
            }

            /// <summary>
            /// Create a horizontal divider in the screen
            /// </summary>
            /// <returns>Divider</returns>
            public static string CreateDivider()
            {
                return new string('=', (int)Math.Round(Console.BufferWidth / 1.25));
            }

            /// <summary>
            /// Create a horizontal divider with a title in the middle
            /// </summary>
            /// <param name="Title">Title</param>
            /// <returns>Divider with title</returns>
            public static string CreateDivider(string title)
            {
                var str = CreateDivider();
                var half = str.Substring(0, (int)Math.Round((decimal)str.Length / 2));
                return $"{half} {title} {half}";
            }

            /// <summary>
            /// Center a string input
            /// </summary>
            /// <param name="Input">Input</param>
            /// <returns>Centered string</returns>
            public static string Center(string input, int width = 0)
            {
                return $"{new string(' ', (width == 0 ? Console.BufferWidth : width / 2) - (input.Length / 2))}{input}";
            }

            /// <summary>
            /// Add a certain amount of space after a string
            /// </summary>
            /// <param name="Input">Input</param>
            /// <param name="Length">Amount of space</param>
            /// <returns></returns>
            public static string Space(string input, int length)
            {
                return $"{input}{new string(' ', (length - input.Length > 0 ? length - input.Length : 0))}";
            }

            public static string HorizontalRainbow(string input, int rotation = 0, int? offset = null)
            {
                if (offset == null)
                    offset = StartProperty?.HorizontalColorRotationOffset ?? 2;

                var sb = new StringBuilder();
                foreach (char c in input)
                {
                    ColorManagement.GetInstance().HsvToRgb(rotation, 1, 1, out int r, out int g, out int b);
                    sb.Append(CreateColorString(Color.FromArgb(r, g, b)) + c);
                    rotation += offset ?? 2;
                }

                return sb.ToString();
            }

            public static string VisibleString(string input)
            {
                var regex = new Regex(@"\x1b\[38;2;[0-9]{1,3};[0-9]{1,3};[0-9]{1,3}m");
                return regex.Replace(input, "");
            }
        }

        /// <summary>
        /// This is the timestamp that is assigned to messages
        /// </summary>
        public class MessagePropertyTime
        {
            /// <summary>
            /// Allow cloning
            /// </summary>
            /// <returns>Cloned self</returns>
            public MessagePropertyTime Clone()
            {
                if (this == null)
                    return null;
                else
                    return (MessagePropertyTime)MemberwiseClone();
            }

            /// <summary>
            /// Update the color to updated rainbow value / assigned value
            /// </summary>
            public void UpdateColor()
            {
                // Make sure that there isn't already an assigned color
                if (Color == Color.Empty && StartProperty.DefaultMessageTime?.Color == Color.Empty)
                {
                    ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                    Color = Color.FromArgb(r, g, b);
                }
                else if (StartProperty.DefaultMessageTime?.Color != Color.Empty)
                    Color = StartProperty.DefaultMessageTime.Color;
            }

            /// <summary>
            /// Update the timestamp to be accurate
            /// </summary>
            public void UpdateTime()
            {
                Text = DateTime.Now.ToString("HH:mm:ss.ff");
            }

            /// <summary>
            /// Will the timestamp be shown?
            /// </summary>
            public bool Show = true;

            /// <summary>
            /// The actual text
            /// </summary>
            public string Text = DateTime.Now.ToString("HH:mm:ss.ff");
            
            /// <summary>
            /// The color of the text
            /// </summary>
            public Color Color;
        }

        /// <summary>
        /// The label on messages
        /// </summary>
        public class MessagePropertyLabel
        {
            /// <summary>
            /// Allow cloning
            /// </summary>
            /// <returns>Cloned self</returns>
            public MessagePropertyLabel Clone()
            {
                if (this == null)
                    return null;
                else
                    return (MessagePropertyLabel)MemberwiseClone();
            }

            /// <summary>
            /// Default text formatting and coloring for ease
            /// </summary>
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

            /// <summary>
            /// Attempt to autoformat input text
            /// </summary>
            public void AutoFormat()
            {
                WordToColorDict.TryGetValue(Text.ToLower(), out object[] val);

                // make sure its not null
                if (val == null)
                    return;

                Color = (Color)val[0];
                Text = val[1].ToString().ToUpper();
            }

            /// <summary>
            /// Should the label be shown?
            /// </summary>
            public bool Show = true;

            /// <summary>
            /// The actual text on the label
            /// </summary>
            public string Text;

            /// <summary>
            /// The color of the label
            /// </summary>
            public Color Color;
        }

        /// <summary>
        /// The offset to dock some text
        /// </summary>
        public class DockOffset
        {
            /// <summary>
            /// Amount from the top
            /// </summary>
            public int Top = 0;

            /// <summary>
            /// Amount from the bottom
            /// </summary>
            public int Bottom = 0;
        }

        /// <summary>
        /// Message properties for writing out
        /// </summary>
        public class MessageProperties
        {
            /// <summary>
            /// Parse the parameters into color groups
            /// </summary>
            /// <param name="messageOrColor">Message or color</param>
            public void Parse(object[] messageOrColor)
            {
                // Color fallback
                var color = Color.FromArgb(200, 200, 200);

                // Iterate through each parameter
                foreach (var param in messageOrColor)
                {
                    // Wrap in try just in case
                    try
                    {
                        // Fallback.
                        if (param == null)
                            color = Color.FromArgb(200, 200, 200);

                        // This means that the parameter is a hex color code and should be interpreted as such
                        else if (param is string && param.ToString().StartsWith("#") && param.ToString().Length == 7)
                        {
                            int argb = int.Parse(param.ToString().Substring(1), System.Globalization.NumberStyles.HexNumber);
                            color = Color.FromArgb(argb);
                        }

                        // Check if it's just a color
                        else if (param is Color)
                            color = (Color)param;

                        // It's just a regular string to be written out
                        else if (param is string)
                        {
                            ColoringGroups.Add(new object[] { color, (string)param });
                            TextLength += param.ToString().Length;
                        }
                    }
                    catch { } // Fuck errors. All the homies hate errors!
                }

                // Autoformat
                if (Label != null)
                    Label.AutoFormat();
            }

            public MessageProperties(params object[] messageOrColor)
            {
                Parse(messageOrColor);

                // Set the time and label
                // Clone so that we can have a default time and message style
                //if (StartProperty.DefaultMessageTime != null)
                    Time = StartProperty.DefaultMessageTime?.Clone();
                //else
                    //Time = new MessagePropertyTime();

                //if ( StartProperty.DefaultMessageLabel != null)
                    Label = StartProperty.DefaultMessageLabel?.Clone();
                //else
                    //Label = new MessagePropertyLabel();
            }

            /// <summary>
            /// Remove all coloring groups, and just return the readable content
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                // Create a string builder to append onto
                var sb = new StringBuilder();

                // Iterate through each coloring group
                ColoringGroups.ForEach(item => {
                    // Find all the strings in the coloring group
                    var allStrings = Array.FindAll(item, item2 => item2 is string);

                    // Append said strings to the string builder
                    sb.Append(string.Join("", allStrings));
                });

                // Return the strings as one.
                return sb.ToString();
            }

            /// <summary>
            /// All the coloring groups
            /// </summary>
            public List<object[]> ColoringGroups = new List<object[]>();

            /// <summary>
            /// Auto wrap text onto new lines?
            /// </summary>
            public bool WordWrap = true;

            /// <summary>
            /// Horizontal rainbow across each character
            /// </summary>
            public bool HorizontalRainbow = false;

            /// <summary>
            /// Vertical rainbow across lines
            /// </summary>
            public bool VerticalRainbow = false;

            /// <summary>
            /// Show the command header after
            /// </summary>
            public bool ShowHeaderAfter = false;

            /// <summary>
            /// Center the text
            /// </summary>
            public bool Center = false;

            /// <summary>
            /// Bypass the console lock (dangerous, mainly used internally)
            /// </summary>
            public bool BypassLock = false;

            /// <summary>
            /// Do not add a new line after write
            /// </summary>
            public bool NoNewLine = false;

            /// <summary>
            /// Total text length
            /// </summary>
            public int TextLength = 0;

            /// <summary>
            /// Forced Y coordinate on the screen
            /// </summary>
            public int? YCood = null;

            /// <summary>
            /// Time label
            /// </summary>
            public MessagePropertyTime Time;

            /// <summary>
            /// Text label
            /// </summary>
            public MessagePropertyLabel Label;

            /// <summary>
            /// Dock offset
            /// </summary>
            public DockOffset DockOffset = new DockOffset();
        }

        /// <summary>
        /// Command header properties
        /// </summary>
        //public class StartupInterfaceProperties
        //{
        //    /// <summary>
        //    /// Username to show
        //    /// </summary>
        //    public string Username = Environment.UserName;

        //    /// <summary>
        //    /// Username color
        //    /// </summary>
        //    public Color UserColor = Color.FromArgb(3, 84, 204);

        //    /// <summary>
        //    /// Host to show
        //    /// </summary>
        //    public string Host = Environment.MachineName;

        //    /// <summary>
        //    /// Host color to show
        //    /// </summary>
        //    public Color HostColor = Color.FromArgb(100, 7, 247);

        //    /// <summary>
        //    /// Automatically print on the next line
        //    /// </summary>
        //    public bool ShowNextLine = false;
        //}

        /// <summary>
        /// Author properties for MOTD
        /// </summary>
        public class StartupAuthorProperties
        {
            /// <summary>
            /// Author name
            /// </summary>
            public string Name;

            /// <summary>
            /// Author URL
            /// </summary>
            public string Url;
        }

        /// <summary>
        /// Console title properties
        /// </summary>
        public class StartupConsoleTitleProperties
        {
            /// <summary>
            /// Animate the title
            /// </summary>
            public bool Animated = false;

            /// <summary>
            /// Delay between characters
            /// </summary>
            public int AnimateDelay = 250;

            /// <summary>
            /// Default text
            /// </summary>
            public string Text = Console.Title;

            /// <summary>
            /// Current status to display
            /// </summary>
            public string Status;
        }

        /// <summary>
        /// Splash screen properties
        /// </summary>
        public class StartupSpashScreenProperties
        {
            public StartupSpashScreenProperties()
            {
                ProgressBarSettings = new ProgressBar.Settings();
            }

            /// <summary>
            /// Auto generate one based on author properties and the logo
            /// </summary>
            public bool AutoGenerate = false;

            /// <summary>
            /// Auto center it
            /// </summary>
            public bool AutoCenter = true;
            
            /// <summary>
            /// Display how long until it disappears
            /// </summary>
            public bool DisplayProgressBar = false;

            /// <summary>
            /// Settings for the shown progress bar
            /// </summary>
            public ProgressBar.Settings ProgressBarSettings;
            
            /// <summary>
            /// How long to display the splash screen for (MS)
            /// </summary>
            public int DisplayTime = 5000;

            /// <summary>
            /// The content to show
            /// </summary>
            public string Content;
        }

        /// <summary>
        /// Message Of The Day properties
        /// </summary>
        public class StartupMOTDProperties
        {
            /// <summary>
            /// MOTD text
            /// </summary>
            public string Text;

            /// <summary>
            /// Divider color
            /// </summary>
            public Color? DividerColor = null;

            /// <summary>
            /// Text color
            /// </summary>
            public Color TextColor = Color.White;
        }

        /// <summary>
        /// Actual start properties
        /// </summary>
        public class StartupProperties
        {
            public StartupProperties()
            {
                Title = new StartupConsoleTitleProperties();
                Author = new StartupAuthorProperties();
                //UserInformation = new StartupInterfaceProperties();
                SplashScreen = new StartupSpashScreenProperties();
                DefaultMessageLabel = new MessagePropertyLabel();
                DefaultMessageTime = new MessagePropertyTime();
                MOTD = new StartupMOTDProperties();
            }

            /// <summary>
            /// Console title properties
            /// </summary>
            public StartupConsoleTitleProperties Title;

            /// <summary>
            /// Author properties
            /// </summary>
            public StartupAuthorProperties Author;

            /// <summary>
            /// Command header properties
            /// </summary>
            //public StartupInterfaceProperties UserInformation;
            
            /// <summary>
            /// Splash screen properties
            /// </summary>
            public StartupSpashScreenProperties SplashScreen;

            /// <summary>
            /// Default message label properties
            /// </summary>
            public MessagePropertyLabel DefaultMessageLabel;

            /// <summary>
            /// Default message time properties
            /// </summary>
            public MessagePropertyTime DefaultMessageTime;

            /// <summary>
            /// Message Of The Day properties
            /// </summary>
            public StartupMOTDProperties MOTD;

            /// <summary>
            /// Logo as a string
            /// </summary>
            public string LogoString;

            /// <summary>
            /// Current version
            /// </summary>
            public string Version = null;

            /// <summary>
            /// Auto size the console
            /// </summary>
            public bool AutoSize = true;

            /// <summary>
            /// Auto increment the version displayed based on AssemblyInfo
            /// </summary>
            public bool UseAutoVersioning = false;

            /// <summary>
            /// Use debug mode
            /// </summary>
            public bool DebugMode = false;

            /// <summary>
            /// Starting color rotation in HSV
            /// </summary>
            public int ColorRotation = 0;

            /// <summary>
            /// Offset each time something is written
            /// </summary>
            public int ColorRotationOffset = 5;

            /// <summary>
            /// Offset each for each character's color when using HorizontalRainbow property
            /// </summary>
            public int HorizontalColorRotationOffset = 2;
        }

        private static Core inst = null;
        /// <summary>
        /// Get an instance
        /// </summary>
        public static Core GetInstance()
        {
            if (inst == null)
                inst = new Core();
            return inst;
        }

        // Import all DLLs required for console modification
        // https://stackoverflow.com/a/43321133/14257203
        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        /// <summary>
        /// The queue to print
        /// </summary>
        public static List<MessageProperties> WriteQueue;
        
        /// <summary>
        /// Startup properties
        /// </summary>
        public static StartupProperties StartProperty = null;

        /// <summary>
        /// Pause console printing
        /// </summary>
        public static bool PauseConsole = false;

        /// <summary>
        /// Was the command header printed last
        /// </summary>
        public static bool HeaderPrintedLast = false;

        /// <summary>
        /// Prevent new items from displaying
        /// </summary>
        public static bool NewItemLock = false;

        //public static int DeadspaceTop = 0;

        /// <summary>
        /// Allow for a deadspace at the bottom of the screen
        /// </summary>
        public static int DeadspaceBottom = 0;

        /// <summary>
        /// Current Y position for the console
        /// </summary>
        public static int CursorY = 0;

        /// <summary>
        /// The saved start position
        /// </summary>
        private static int colorRotationStart = 0;

        /// <summary>
        /// Worker thread for printing
        /// </summary>
        private Thread workThread;

        // Delegates
        public delegate void _noReturn();
        public delegate void _workItem(MessageProperties Message);

        /// <summary>
        /// Fires on console clear
        /// </summary>
        public static event _noReturn OnClear;

        /// <summary>
        /// Fires on queue clearing
        /// </summary>
        public static event _noReturn QueueCleared;

        /// <summary>
        /// Fires on new item to write
        /// </summary>
        public static event _workItem ItemAddedToQueue;

        /// <summary>
        /// Fires on write finish
        /// </summary>
        public static event _workItem ItemFinished;

        /// <summary>
        /// Start this library
        /// </summary>
        /// <param name="startProperties">Custom start properties</param>
        public void Start(StartupProperties startProperties = null)
        {
            // Clear the console
            Console.Clear();
            Debug.WriteLine("STARTING...");

            // Setup all required parts
            StartProperty = startProperties == null ? new StartupProperties() : startProperties;
            WriteQueue = new List<MessageProperties>();
            colorRotationStart = StartProperty.ColorRotation;

            // Set the console mode to accept color tags
            // https://stackoverflow.com/a/43321133/14257203
            var Handle = GetStdHandle(-11);
            uint Mode;
            GetConsoleMode(Handle, out Mode);
            SetConsoleMode(Handle, Mode | 0x4);

            // Fetch version
            if (StartProperty.UseAutoVersioning)
                StartProperty.Version = GetAutoVersion().ToString();

            // Find the longest part of the logo for auto sizing
            int longestLen = 0;
            if (StartProperty.LogoString != null)
                foreach (var line in StartProperty.LogoString.Split('\n'))
                    if (Formatting.VisibleString(line).Length > longestLen)
                        longestLen = Formatting.VisibleString(line).Length;

            // Set console horizontal size, 115, or longest logo line, whichever is longer
            if (StartProperty.AutoSize)
            {
                int wid = longestLen;

                if (wid < 100)
                    wid = 100;
                
                if (longestLen > Console.LargestWindowWidth)
                    wid = Console.LargestWindowWidth;

                Console.WindowWidth = wid;
                Console.BufferWidth = wid;
            }


            // Set the console title and the animation if enabled
            if (StartProperty.Title != null)
            {
                Console.Title = StartProperty.Title.Text;

                // If enabled, animate
                if (StartProperty.Title.Animated)
                    new Thread(animatedTitleLoop).Start();
            }

            // Start work thread
            workThread = new Thread(workLoop);
            workThread.Start();

            // Show splash screen
            ShowSplash();

            // Print logo / attributions
            PrintLogo();

            // If it's in debug mode, add debug
            if (StartProperty.DebugMode)
            {
                ItemAddedToQueue += (msg) =>
                {
                    Debug.WriteLine($"New item in queue: {JsonConvert.SerializeObject(msg)}");
                };
            }
        }

        /// <summary>
        /// Clear the console
        /// </summary>
        /// <param name="showLogo">Show the logo after clearing</param>
        public void Clear(bool showLogo = true)
        {
            // Clear the write queue
            if (WriteQueue.Count > 0)
                WriteQueue.Clear();

            // Turn off showing the next line
            //prevTog = StartProperty.UserInformation.ShowNextLine;
            //if (prevTog)
            //    StartProperty.UserInformation.ShowNextLine = false;

            // Reset the current color rotation to the original one
            StartProperty.ColorRotation = colorRotationStart;

            // Actually clear the console and reset the cursor's Y
            Console.Clear();
            CursorY = 0;

            // Print the logo if enabled
            if (showLogo)
                PrintLogo();

            // Fire event
            OnClear?.Invoke();
        }

        /// <summary>
        /// Print the command header
        /// </summary>
        //public void PrintHeader()
        //{
        //    // Make sure nothing is going to mess it up
        //    while (WriteQueue.Count > 0)
        //        Thread.Sleep(5);

        //    // Just formatting
        //    Console.Write($"\r{Formatting.CreateColorString(StartProperty.UserInformation.UserColor)}{StartProperty.UserInformation.Username}");
        //    Console.ResetColor();
        //    Console.Write($"@{Formatting.CreateColorString(StartProperty.UserInformation.HostColor)}{StartProperty.UserInformation.Host}");
        //    Console.ResetColor();
        //    Console.Write(" #~ ");

        //    // Update var for internal processing
        //    HeaderPrintedLast = true;
        //}

        /// <summary>
        /// Get version from AssemblyInfo, if you use '*' in the file, it will auto increment.
        /// </summary>
        /// <returns>Version in AssemblyInfo</returns>
        public Version GetAutoVersion()
        {
            return GetType().Assembly.GetName().Version;
        }

        // Current status
        private string titleStatus = "";

        /// <summary>
        /// Update the title's current status
        /// </summary>
        /// <param name="status">New status</param>
        public void UpdateTitleStatus(string status)
        {
            titleStatus = status;
            Console.Title = $"{StartProperty.Title.Text} | {status}";
        }

        /// <summary>
        /// Animate the title logo
        /// </summary>
        private void animatedTitleLoop()
        {
            // String builder for the chars
            StringBuilder sb = new StringBuilder();

            // This determines what way it will be animating in
            bool mode = true;
            while (true)
            {
                // Switch for the mode, true = out, false = in
                if (mode)
                {
                    // Clear the current string builder
                    sb.Clear();

                    // Add each character one by one
                    foreach (var c in StartProperty.Title.Text)
                    {
                        // Append to string builder
                        sb.Append(c);

                        // Set and sleep
                        Console.Title = $"{sb} | {titleStatus}";
                        Thread.Sleep(StartProperty.Title.AnimateDelay);
                    }

                    // Invert the mode to go back in
                    mode = false;

                    // Slight delay so it looks nicer
                    Thread.Sleep(StartProperty.Title.AnimateDelay);
                }
                else
                {
                    // Remove each character one by one
                    for (var x = StartProperty.Title.Text.Length; x > 0; x--)
                    {
                        // Remove the last char
                        string ttl = Console.Title.Substring(0, Console.Title.Length - (titleStatus.Length + 3));

                        // Set and sleep
                        Console.Title = $"{ttl.Substring(0, x - 1)} | {titleStatus}";
                        Thread.Sleep(StartProperty.Title.AnimateDelay);
                    }

                    // Invert mode to go back out
                    mode = true;
                }
            }
        }

        /// <summary>
        /// Print logo and attributions
        /// </summary>
        public void PrintLogo()
        {
            // Write each line of the logo if it's set
            if (StartProperty.LogoString != null)
                foreach (var line in StartProperty.LogoString.Split('\n'))
                    WriteLine(new MessageProperties { Label = null, Time = null, VerticalRainbow = true }, line);

            // Print all attributions
            if (StartProperty.Author.Name != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Made by {StartProperty.Author.Name}");
            if (StartProperty.Author.Url != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Author URL: {StartProperty.Author.Url}");
            if (StartProperty.Version != null)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, $" > Version {StartProperty.Version}");
            if (StartProperty.DebugMode)
                WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true }, " > Debug mode enabled");

            // Extra line for spice
            WriteLine();

            // If an MOTD is supplied, print it
            //if (StartProperty.MOTD != null && StartProperty.MOTD.Text.Length > 0)
                PrintMOTD();
            //else if (prevTog)
                //StartProperty.UserInformation.ShowNextLine = true;
        }

        /// <summary>
        /// Print the MOTD
        /// </summary>
        public void PrintMOTD()
        {
            if (StartProperty.MOTD.Text == null)
                return;

            // Get all the segments of the dividers
            string div = Formatting.CreateDivider();
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));
            string divColor = StartProperty.MOTD.DividerColor == null ? "" : Formatting.CreateColorString(StartProperty.MOTD.DividerColor ?? Color.White);

            // Horizontal rainbow only if a color is not supplied
            bool rb = false;
            if (StartProperty.MOTD.DividerColor == null)
                rb = true;

            // Divider
            WriteLine(new MessageProperties { HorizontalRainbow = rb, Label = null, Time = null, Center = true }, $"{divColor}{divHalf} MOTD {divHalf}");
            WriteLine();

            // Content
            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, StartProperty.MOTD.TextColor, StartProperty.MOTD.Text);

            // Divider
            WriteLine();
            WriteLine(new MessageProperties { HorizontalRainbow = rb, Label = null, Time = null, Center = true }, $"{divColor}{divHalf} MOTD {divHalf}");

            // Show the command header after if enabled
            //if (StartProperty.UserInformation.ShowNextLine)
            //    WriteLine(new MessageProperties { ShowHeaderAfter = true });
            //else
                WriteLine();
        }

        /// <summary>
        /// Show the splash screen animation
        /// </summary>
        public void ShowSplash()
        {
            // If there is no splash screen, just return
            if (StartProperty.SplashScreen == null || StartProperty.LogoString == null)
                return;

            // Pause the console so that no one will try to print
            bool paused = PauseConsole;
            PauseConsole = true;

            // Disable cursor visibility
            var visible = Console.CursorVisible;
            Console.CursorVisible = false;

            // Clear the console so it's a blank slate
            Clear();

            // Find out how much needs to go into centering the logo
            int centerAmnt = 0;
            foreach (var line in StartProperty.LogoString.Split('\n'))
            {
                // If it's less than, it needs to be updated
                if (line.Length > centerAmnt)
                    centerAmnt = line.Length;
            }

            // Division to find out how many spaces would need to be inserted
            centerAmnt = (Console.BufferWidth / 2) - (centerAmnt / 2);

            // Make sure it's still positive
            if (centerAmnt < 0)
                centerAmnt = 0;

            // Auto generation is handled here
            if (StartProperty.SplashScreen.AutoGenerate)
            {
                // Vertical centering
                WriteLine(new MessageProperties { Label = null, Time = null, BypassLock = true }, new string('\n', (Console.WindowHeight / 2) - (StartProperty.LogoString.Split('\n').Length / 2) - (StartProperty.Author != null ? 1 : 0)));

                // Printing the logo out
                foreach (var line in StartProperty.LogoString.Split('\n'))
                    WriteLine(new MessageProperties { Label = null, Time = null, VerticalRainbow = true, BypassLock = true }, $"{new string(' ', centerAmnt)}{line}");

                // Empty line
                WriteLine(new MessageProperties { Label = null, Time = null, BypassLock = true }, "");

                // Writing who it was made by
                if (StartProperty.Author.Name != null)
                    WriteLine(new MessageProperties { Label = null, Time = null, HorizontalRainbow = true, Center = true, BypassLock = true }, $"Made by {StartProperty.Author.Name}");
            }

            // The time in milliseconds it started
            long timeStarted = General.EpochTimeMilliseconds;

            // Time it will end in milliseconds
            long timeEnd = timeStarted + StartProperty.SplashScreen.DisplayTime;

            // If we are going to display the progress bar, start a new render thread for it
            if (StartProperty.SplashScreen.DisplayProgressBar)
            {
                new Thread(() => {
                    // Force the total parts to 100
                    StartProperty.SplashScreen.ProgressBarSettings.TotalParts = 100;

                    // Instantiate a new bar
                    var pb = new ProgressBar(StartProperty.SplashScreen.ProgressBarSettings);

                    // Keep looping while the current time is less than the end time
                    while (General.EpochTimeMilliseconds < timeEnd)
                    {
                        // Set the progress and render it
                        pb.SetProgress(((int)(General.EpochTimeMilliseconds - timeStarted)) * 100 / StartProperty.SplashScreen.DisplayTime);
                        pb.Render();

                        // Sleep
                        Thread.Sleep(50);
                    }

                    // Remove the progress bar after being finished
                    pb.Remove();
                }).Start();
            }

            // Wait until the splash screen is done
            Thread.Sleep(StartProperty.SplashScreen.DisplayTime);

            // Clear the console
            Clear(false);

            // Restore the old state
            PauseConsole = paused;
            Console.CursorVisible = visible;
        }

        /// <summary>
        /// Create a new alert
        /// </summary>
        /// <param name="title">Title</param>
        /// <param name="divider">Divider color</param>
        /// <param name="lines">Lines of text</param>
        public void CreateAlert(string title, Color divider, params string[] lines)
        {
            // Create the divider
            string div = Formatting.CreateDivider();

            // Get half the divider
            string divHalf = div.Substring(0, (int)Math.Round((decimal)div.Length / 2));

            // Divider
            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, divider, $"{divHalf} {title} {divHalf}");
            WriteLine();

            // Content
            foreach (var line in lines)
                WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, line);

            // Divider
            WriteLine();
            WriteLine(new MessageProperties { Label = null, Time = null, Center = true }, divider, $"{divHalf} {title} {divHalf}");
            WriteLine();
        }

        /// <summary>
        /// Parse parameters into MessageProperties class
        /// </summary>
        /// <param name="messageOrColor">Parameters</param>
        /// <param name="properties">MessageProperties</param>
        private void parseWrite(object[] messageOrColor, MessageProperties properties = null)
        {
            // If there is no start property, then the UI was never started
            if (StartProperty == null)
                Start();

            // Make sure there's some properties
            if (properties == null)
                properties = new MessageProperties(StartProperty.ColorRotation);
            if (messageOrColor != null)
                properties.Parse(messageOrColor);

            // If they are not bypassing lock, Wait until processing
            while (NewItemLock && !properties.BypassLock)
                Thread.Sleep(100);

            // Add the properties to the queue
            WriteQueue.Add(properties);

            // Fire items added event
            if (ItemAddedToQueue != null)
                lock (ItemAddedToQueue)
                    ItemAddedToQueue?.Invoke(properties);
        }

        /// <summary>
        /// Write text to console
        /// </summary>
        /// <param name="properties">Message properties</param>
        /// <param name="messageOrColor">Content</param>
        public void WriteLine(MessageProperties properties, params object[] messageOrColor)
        {
            // Parse the content
            parseWrite(messageOrColor, properties);
        }

        /// <summary>
        /// Write text to console
        /// </summary>
        /// <param name="messageOrColor">Content</param>
        public void WriteLine(params object[] messageOrColor)
        {
            // Parse content
            parseWrite(messageOrColor);
        }

        /// <summary>
        /// Write text to console
        /// </summary>
        /// <param name="properties">MessageProperties</param>
        public void WriteLine(MessageProperties properties)
        {
            // Make sure no lock in in place
            while (NewItemLock && !properties.BypassLock)
                Thread.Sleep(100);

            // Make sure that coloring groups is not null
            if (properties.ColoringGroups != null)
                WriteQueue.Add(properties);
        }

        /// <summary>
        /// Write blank line to console
        /// </summary>
        public void WriteLine()
        {
            // Make sure there's not a lock
            while (NewItemLock)
                Thread.Sleep(100);

            // Enqueue a blank item
            WriteQueue.Add(new MessageProperties { Label = new MessagePropertyLabel { Show = false }, Time = new MessagePropertyTime { Show = false }, ColoringGroups = new List<object[]> { new object[] { null } } });
        }

        /// <summary>
        /// Good for find and replacing color tags
        /// </summary>
        Regex colorStringRegex = new Regex(@"(\x1b)[\[\]0-9;]{0,99}m");

        /// <summary>
        /// Previous Y on set
        /// </summary>
        static int prevY = 0;
        internal static void setWindow()
        {
            // Get the current scroll position
            int y = CursorY - (Console.WindowHeight - DeadspaceBottom);

            // Set the cursor position if deadspace is enabled
            if (DeadspaceBottom > 0 && y > 0)
                Console.SetWindowPosition(0, y);
            else if (DeadspaceBottom == 0)
                prevY = Console.WindowTop;
        }

        /// <summary>
        /// The main loop for printing
        /// </summary>
        private void workLoop()
        {
            // Never stop.
            while (true)
            {
                // Wrap to ensure no crashing
                try
                {
                    // Make sure theres work to do
                    if (WriteQueue.Count == 0)
                        continue;

                    // Set the scroll
                    setWindow();

                    // Get the next properties
                    int index = 0;
                    MessageProperties properties = WriteQueue[index];

                    // Make sure console isn't paused
                    if (PauseConsole)
                    {
                        // Get all items that can bypass lock
                        var filtered = WriteQueue.FindAll(item => item != null && item.BypassLock);

                        // If there is an item, set the propeties to that item and keep going
                        if (filtered.Count > 0)
                        {
                            properties = filtered[0];
                            index = WriteQueue.IndexOf(filtered[0]);
                        }
                        else
                            continue; 
                    }

                    // Property validation
                    if (properties == null)
                    {
                        WriteQueue.RemoveAt(index);
                        continue;
                    }
                    else if (properties.ShowHeaderAfter && properties.ColoringGroups != null && properties.ColoringGroups.Count == 0)
                    {
                        //PrintHeader();
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

                    // Step up the color rotation
                    StartProperty.ColorRotation += StartProperty.ColorRotationOffset;

                    // Reset color rotation
                    if (StartProperty.ColorRotation >= 360)
                        StartProperty.ColorRotation = 0;

                    // Make sure to prevent buffer overflows
                    if (CursorY >= Console.BufferHeight - 10)
                    {
                        Clear();
                        CursorY = 0;
                    }

                    // Set cursor position
                    if (properties.DockOffset.Top > 0)
                        Console.SetCursorPosition(0, Console.WindowTop + properties.DockOffset.Top);
                    else if (properties.DockOffset.Bottom > 0)
                        Console.SetCursorPosition(0, Console.WindowTop + Console.WindowHeight - properties.DockOffset.Bottom);
                    else if (properties.YCood == null)
                        Console.SetCursorPosition(0, CursorY);
                    else
                        Console.SetCursorPosition(0, properties.YCood ?? 0);

                    // Write out the timestamp
                    if (properties.Time != null && properties.Time.Show)
                    {
                        // Update the coloring
                        properties.Time.UpdateColor();
                        properties.Time.UpdateTime();

                        // Write
                        Console.ResetColor();
                        Console.Write("\r[");
                        Console.Write($"{Formatting.CreateColorString(properties.Time.Color)}{properties.Time.Text}");
                        Console.ResetColor();
                        Console.Write("] ");
                    }
                    else
                        Console.Write("\r");

                    // Reset
                    Console.ResetColor();

                    // If the message is centered, do this mess
                    if (properties.Center)
                    {
                        int totalLen = 0;
                        foreach (var grp in properties.ColoringGroups)
                        {
                            // Remove all color tags from the string
                            foreach (Match match in colorStringRegex.Matches(grp[1].ToString().ToLower()))
                                totalLen -= match.Length;

                            totalLen += grp[1].ToString().Length;
                        }
                        
                        // Create the true center
                        int cnt = (int)Math.Round((decimal)(Console.BufferWidth / 2) - (totalLen / 2)) - (properties.Time != null && properties.Time.Show ? properties.Time.Text.Length + 3 : 0);
                        Console.Write(new string(' ', cnt < 0 ? 0 : cnt));
                    }

                    // Coloring
                    if (!properties.HorizontalRainbow && !properties.VerticalRainbow)
                    {
                        // Iterate through each coloring group
                        foreach (var grp in properties.ColoringGroups)
                        {
                            // Fallback color
                            Color clr = Color.WhiteSmoke;

                            // If the color is 'rainbow', get the color
                            if (grp[0].ToString() == "rainbow")
                            {
                                ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                                clr = Color.FromArgb(r, g, b);
                            }

                            // If its a color, set it to that
                            if (grp[0] is Color)
                                clr = (Color)grp[0];

                            // Set the color in console
                            Console.Write($"{Formatting.CreateColorString(clr)}{grp[1]}");
                        }
                    } // Horizontal rainbow work
                    else if (properties.HorizontalRainbow)
                    {
                        // String builder
                        StringBuilder sb = new StringBuilder();

                        // Get all text
                        foreach (var grp in properties.ColoringGroups)
                            sb.Append(grp[1]);

                        // Get all characters and color tag them
                        foreach (var c in sb.ToString())
                        {
                            ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                            Console.Write($"{Formatting.CreateColorString(Color.FromArgb(r, g, b))}{c}");

                            // Increase color rotation just one.
                            StartProperty.ColorRotation += StartProperty.HorizontalColorRotationOffset;
                        }
                    } // Vertical rainbow work
                    else if (properties.VerticalRainbow)
                    {
                        // String builder
                        StringBuilder sb = new StringBuilder();

                        // Get all text
                        foreach (var grp in properties.ColoringGroups)
                            sb.Append(grp[1]);

                        // Tag each line
                        ColorManagement.GetInstance().HsvToRgb(StartProperty.ColorRotation, 1, 1, out int r, out int g, out int b);
                        Console.Write($"{Formatting.CreateColorString(Color.FromArgb(r, g, b))}{sb}");
                    }

                    // Adjust the cursors Y position, used for word wrap, calculates based on length and buffer width
                    int total = 0;
                    if (properties.Label != null && properties.Label.Show)
                        total += properties.Label.Text.Length + 3;
                    if (properties.Time != null && properties.Time.Show)
                        total += properties.Time.Text.Length + 3;

                    // If there is a new line after the message, add to the cursor's Y position
                    if (!properties.NoNewLine)
                        CursorY += properties.ToString().Split('\n').Length;

                    // Write the label finally
                    if (properties.Label != null && properties.Label.Show)
                    {
                        // Put it on the side and align
                        Console.Write(new string(' ', Console.BufferWidth - properties.Label.Text.Length - 4 - Console.CursorLeft));
                        Console.CursorLeft = Console.BufferWidth - properties.Label.Text.Length - 4;

                        // Formatting
                        Console.ResetColor();
                        Console.Write(" [");
                        Console.Write($"{Formatting.CreateColorString(properties.Label.Color)}{properties.Label.Text}");
                        Console.ResetColor();
                        Console.Write("]");
                    }

                    // It was no longer the last item printed
                    HeaderPrintedLast = false;

                    // Final line if enabled
                    if (!properties.NoNewLine)
                    {
                        Console.WriteLine();
                        Debug.WriteLine("");
                    }

                    // Final color reset
                    Console.ResetColor();

                    // Make sure it's not just going to remove nothing
                    if (WriteQueue.Count > 0)
                        WriteQueue.RemoveAt(index);

                    // Fire queue cleared event if nothing is left
                    if (WriteQueue.Count == 0)
                        QueueCleared?.Invoke();

                    // If the command header is not blank, write it
                    //if (StartProperty.UserInformation != null && StartProperty.UserInformation.ShowNextLine && WriteQueue.Count == 0)
                    //        PrintHeader();

                    // Finally reset scroll again
                    setWindow();

                    // Fire item finished event
                    ItemFinished?.Invoke(properties);
                }
                catch (Exception ex)
                {
                    // Remove errored item
                    if (WriteQueue.Count > 0)
                        WriteQueue.RemoveAt(0);

                    // Write error
                    Debug.WriteLine(ex);
                    Debug.WriteLine(new StackTrace());
                }
            }
        }

        /// <summary>
        /// Read user input
        /// </summary>
        /// <param name="pre">Pretext</param>
        /// <param name="inputColor">Input color</param>
        /// <param name="startingPos">Starting position</param>
        /// <returns>User input</returns>
        public string ReadLine(string pre = null, Color? inputColor = null, int startingPos = 0)
        {
            // Make sure nothing is being written
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            // Make sure Y position is valid
            if (CursorY < 0)
                CursorY = 0;

            // Set the cursor position
            Console.SetCursorPosition(startingPos, CursorY);

            // Increase so it doesn't overwrite
            CursorY++;

            // Write the pretext
            if (pre != null)
                Console.Write(pre);

            // Set the color if enabled
            if (inputColor != null)
                Console.Write(Formatting.CreateColorString(inputColor ?? Color.White));

            // Return the user input
            return Console.ReadLine();
        }

        /// <summary>
        /// Read user input but replaces user input with *
        /// </summary>
        /// <param name="pre">Pretext</param>
        /// <param name="inputColor">Input color</param>
        /// <param name="startingPos">Starting position</param>
        /// <returns>User input</returns>
        public string ReadLineProtected(string pre = null, Color? inputColor = null, int startingPos = 0)
        {
            // String builder
            StringBuilder sb = new StringBuilder();

            // Write the pretext if enabled
            if (pre != null)
                Console.Write(pre);

            // Set the input color if enabled
            if (inputColor != null)
                Console.Write(Formatting.CreateColorString(inputColor ?? Color.White));

            // While loop so we can readkey and set * after each char
            while (true)
            {
                // Get pressed key
                var key = Console.ReadKey();

                // Backspace pressed
                if (key.Key == ConsoleKey.Backspace)
                {
                    // Do not go back further
                    if (sb.Length == 0)
                    {
                        Console.Write(" ");
                        continue;
                    }

                    // Remove last char from SB
                    sb.Remove(sb.Length - 1, 1);
                    
                    // Set position and write ' '
                    Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length), CursorY);
                    Console.Write(" ");
                    Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length), CursorY);
                    continue;
                } // Enter pressed
                else if (key.Key == ConsoleKey.Enter) // Return string if finished
                {
                    // Increase one and return the string
                    CursorY++;
                    return sb.ToString();
                }

                // If it is not backspace or enter, add it to the string
                sb.Append(key.KeyChar);

                // Set the cursor position and overwrite the character with a *
                Console.SetCursorPosition((startingPos > 0 ? startingPos : pre.Length) + (sb.Length - 1), CursorY);
                Console.Write("*");
            }
        }

        /// <summary>
        /// Start delay after queue is cleared
        /// </summary>
        /// <param name="ms">Milliseconds</param>
        public void Delay(int ms)
        {
            // Make sure queue is clear
            while (WriteQueue.Count > 0)
                Thread.Sleep(5);

            // Sleep
            Thread.Sleep(ms);
        }
    }
}
