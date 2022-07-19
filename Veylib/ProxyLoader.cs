using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;

namespace Veylib
{
    public class ProxyLoader
    {
        /// <summary>
        /// Proxy list
        /// </summary>
        public HashSet<WebProxy> Proxies = new HashSet<WebProxy>();

        // This is the current proxy index
        private int proxyIndex = 0;

        /// <summary>
        /// Load proxies from file
        /// </summary>
        /// <param name="filename">File name</param>
        /// <exception cref="FileNotFoundException"></exception>
        public void LoadProxies(string filename = "proxies.txt")
        {
            // Load proxies from lines
            LoadProxies(File.ReadAllLines(Path.Combine(Environment.CurrentDirectory, filename)));
        }

        /// <summary>
        /// Load array of strings to proxies
        /// </summary>
        /// <param name="proxies">Array of proxies</param>
        public void LoadProxies(string[] proxies)
        {
            // Iterate through each
            foreach (var proxy in proxies)
            {
                // Get each field
                var fields = proxy.Split(':');

                // If it's more than 2 fields, it includes username and password
                if (fields.Length > 2)
                    Proxies.Add(new WebProxy($"http://{fields[0]}:{fields[1]}", true, null, new NetworkCredential(fields[2], fields[3])));
                else // Free proxy
                    Proxies.Add(new WebProxy($"http://{fields[0]}:{fields[1]}"));
            }
        }

        /// <summary>
        /// Remove proxies
        /// </summary>
        /// <param name="proxy"></param>
        public void AddProxy(params WebProxy[] proxies)
        {
            foreach (var proxy in proxies)
                Proxies.Add(proxy);
        }

        /// <summary>
        /// Remove a single proxy
        /// </summary>
        /// <param name="proxy">Proxies</param>
        public void RemoveProxy(params WebProxy[] proxies)
        {
            foreach (var proxy in proxies)
               Proxies.Remove(proxy);
        }

        /// <summary>
        /// Test the current proxy list
        /// </summary>
        /// <returns>Working proxies</returns>
        public HashSet<WebProxy> TestProxies()
        {
            return TestProxies(Proxies);
        }

        /// <summary>
        /// Test multiple proxies
        /// </summary>
        /// <param name="proxies">Proxy</param>
        /// <returns>Working proxies</returns>
        public HashSet<WebProxy> TestProxies(params WebProxy[] proxies)
        {
            return TestProxies(proxies.ToHashSet());
        }
        
        /// <summary>
        /// Test a HashSet of proxies
        /// </summary>
        /// <param name="proxies">Proxies</param>
        /// <returns>Working proxies</returns>
        public HashSet<WebProxy> TestProxies(HashSet<WebProxy> proxies)
        {
            // New HashSet
            var working = new HashSet<WebProxy>();

            // Iteration
            foreach (var proxy in proxies)
            {
                // Client to make request with
                var client = WebRequest.Create("http://google.com");
                client.Method = "HEAD"; // Head since we don't need content
                client.Proxy = proxy; // Set the proxy to truely test it

                // No crash.
                try
                {
                    // Get response
                    client.GetResponse();

                    // If it continues, it was successful
                    working.Add(proxy);
                } catch { } // Clearly didn't work if it runs catch
            }

            // Return the working proxies
            return working;
        }

        /// <summary>
        /// Get a proxy in the list
        /// </summary>
        /// <returns>Proxy</returns>
        public WebProxy GetProxy()
        {
            // Get the proxy at the current index
            var prox = Proxies.ElementAt(proxyIndex);

            // Increment by one if it's not going to overflow, else, reset to 0
            if (proxyIndex + 1 > Proxies.Count)
                proxyIndex = 0;
            else
                proxyIndex++;

            // Return proxy that is being used
            return prox;
        }
    }
}
