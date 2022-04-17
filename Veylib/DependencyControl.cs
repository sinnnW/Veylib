using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Veylib
{
    public class DependencyControl
    {
        private static readonly List<string> requiredFiles = new List<string>();
        public static string SiteBase;

        public static void AddFile(string fileName)
        {
            requiredFiles.Add(fileName);
        }

        public static void DownloadFile()
        {
            var wc = new WebClient();
            foreach (var file in requiredFiles)
            {
                try
                {
                    wc.DownloadFile($"{SiteBase}/{file}", Path.Combine(Environment.CurrentDirectory, file));
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to download {file}: {ex}");
                }
            }

            return;
        }
    }
}
