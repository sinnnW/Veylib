using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace Veylib
{
    public class ProxyLoader
    {
        public HashSet<WebProxy> Proxies = new HashSet<WebProxy>();
        private int proxyIndex = 0;

        public void LoadProxies(string filename = "proxies.txt")
        {
            if (!File.Exists(filename))
                throw new FileNotFoundException();

            LoadProxies(File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, filename)));
        }

        public void LoadProxies(string[] proxies)
        {
            foreach (var proxy in proxies)
            {
                var fields = proxy.Split(':');

                if (fields.Length > 2)
                    Proxies.Add(new WebProxy($"http://{fields[0]}:{fields[1]}", true, null, new NetworkCredential(fields[2], fields[3])));
                else
                    Proxies.Add(new WebProxy($"http://{fields[0]}:{fields[1]}"));
            }
        }

        public void AddProxy(WebProxy proxy)
        {
            Proxies.Add(proxy);
        }

        public WebProxy GetProxy()
        {
            var prox = Proxies.ElementAt(proxyIndex);

            if (proxyIndex + 1 > Proxies.Count)
                proxyIndex = 0;
            else
                proxyIndex++;

            return prox;
        }
    }
}
