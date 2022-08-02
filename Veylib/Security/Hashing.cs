using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;

namespace Veylib.Security
{
    public class Hashing
    {
        /// <summary>
        /// Hash a string in SHA256 (https://stackoverflow.com/questions/46194754/how-to-hex-encode-a-sha-256-hash)
        /// </summary>
        /// <param name="input"></param>
        /// <returns>SHA256 hash</returns>
        public static string HashString(string input)
        {
            var sha = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(input));

            var res = new StringBuilder();
            for (var x = 0; x < sha.Length; x++)
                res.Append(sha[x].ToString("X2"));

            return res.ToString();
        }

        /// <summary>
        /// Get a file's checksum in MD5
        /// </summary>
        /// <param name="filepath">Path to file</param>
        /// <returns>MD5 Checksum</returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static string FileChecksum(string filepath)
        {
            if (!File.Exists(filepath))
                throw new FileNotFoundException();

            var md5 = MD5.Create();
            var stream = File.OpenRead(filepath);

            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
        }

        /// <summary>
        /// Cache a list of checksums in the registry
        /// </summary>
        /// <param name="checksums">Dictionary of checksums</param>
        public static void CacheChecksums(Dictionary<string, string> checksums)
        {
            // Open subkey
            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Veylib\Checksums", true);

            // Make sure it's not null, if so, create one
            if (key == null)
                key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Veylib\Checksums", true);

            // Iterate through each loaded module
            foreach (var checksum in checksums)
                key.SetValue(checksum.Key, checksum.Value);
        }
    }
}
