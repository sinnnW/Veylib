using System;
using System.Collections.Generic;
using System.Management;
using System.Net;
using System.IO;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Veylib.Authentication
{
    public static class Shared
    {
        /// <summary>
        /// The URL to the API, must be VelocityAPI V2.
        /// </summary>
        public static string APIUrl;

        /// <summary>
        /// The application ID
        /// </summary>
        public static int AppID = -1;

        /// <summary>
        /// This is the machines HWID
        /// </summary>
        public static string HWID
        {
            get
            {
                var mos = new ManagementObjectSearcher("Select ProcessorId From Win32_Processor");
                var mosList = mos.Get();
                foreach (var mo in mosList)
                {
                    return mo["ProcessorId"].ToString();
                }
                return "";
            }
        }

        /// <summary>
        /// Generate a basic web request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static WebRequest GenerateWR(string url)
        {
            WebRequest req = WebRequest.Create($"{APIUrl}/{url}");

            if (User.CurrentUser != null)
                req.Headers.Add("Authorization", User.CurrentUser.Token);

            req.Headers.Add("HWID", HWID);

            return req;
        }

        /// <summary>
        /// Read and return a dynamic
        /// </summary>
        /// <param name="req">The request</param>
        /// <returns>Dynamic of the JSON returned</returns>
        public static dynamic ReadResponse(WebRequest req)
        {
            try
            {
                var resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
                var json = JsonConvert.DeserializeObject<dynamic>(resp);

                return json;
            } catch (WebException ex)
            {
                Debug.WriteLine(new StreamReader(ex.Response.GetResponseStream()).ReadToEnd());
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        public static Dictionary<string, string> OrganizeProperties(dynamic dyn)
        {
            // Setup each of the properties in a more manageable dynamic
            Dictionary<string, string> dict = new Dictionary<string, string>();
            foreach (var property in dyn.extra)
                dict.Add((string)property.key, (string)property.value);

            return dict;
        }

        public static bool LocalDnsPoisened()
        {
            var hosts = File.ReadAllText(@"\Windows\System32\drivers\etc\hosts");
            if (hosts.Contains("verlox.cc"))
                return true;
            
            return false;
        }
    }
}
