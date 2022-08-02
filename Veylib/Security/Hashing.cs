using System.Security.Cryptography;
using System.Text;
using System.IO;
using System;

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
    }
}
