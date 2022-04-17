using System.Security.Cryptography;
using System.Text;

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
    }
}
