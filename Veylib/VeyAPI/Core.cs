using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veylib.VeyAPI
{
    public class Core
    {
        public static string Domain { get; protected set; }
        public static string Authorization { get; protected set; }

        public Core(string domain, string authorization)
        {
            Domain = domain;
            Authorization = authorization;
        }

        internal static string buildUrl(string endpoint)
        {
            if (Domain == null)
                throw new Exception("API core not started");

            return $"https://{Domain}/{endpoint}";
        }
    }
}
