using System.Net;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Veylib.Utilities.Net
{
    public class NetRequest : WebRequest 
    {
        // The request internally used
        internal WebRequest request;

        public NetRequest(string url)
        {
            request = Create(url);
        }

        public NetRequest(Uri uri)
        {
            request = Create(uri);
        }

        /// <summary>
        /// Set request method
        /// </summary>
        /// <param name="method"></param>
        public void SetMethod(Method method)
        {
            request.Method = method.ToString();
        }
        
        /// <summary>
        /// Set a header on the request
        /// </summary>
        /// <param name="key">Header</param>
        /// <param name="value">Value</param>
        public void SetHeader(string key, string value)
        {
            request.Headers.Set(key, value);
        }

        /// <summary>
        /// Remove a header
        /// </summary>
        /// <param name="key">Header</param>
        public void RemoveHeader(string key)
        {
            request.Headers.Remove(key);
        }

        /// <summary>
        /// Set content on request
        /// </summary>
        /// <param name="content"></param>
        public void SetContent(dynamic content)
        {
            // Turn to bytes, then write the stream to the request
            var bytes = Encoding.ASCII.GetBytes(content);
            request.GetRequestStream().Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Set content type on request
        /// </summary>
        /// <param name="contentType"></param>
        public void SetContentType(string contentType)
        {
            request.ContentType = contentType;
        }

        /// <summary>
        /// Send the request and return the responses
        /// </summary>
        /// <returns>Response</returns>
        public NetResponse Send()
        {
            // Doesn't allow crashes.
            WebResponse resp;
            try
            {
                resp = request.GetResponse();
            } catch (WebException ex)
            {
                resp = ex.Response;
            }

            // NetResponse
            var nr = new NetResponse();

            // Fill content
            nr.Content = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            nr.ContentType = resp.ContentType;
            nr.Status = ((HttpWebResponse)resp).StatusCode;
            nr.StatusDescription = ((HttpWebResponse)resp).StatusDescription;

            // Return
            return nr;
        }

        public static void DownloadFile(string url, string path)
        {
            DownloadFile(new Uri(url), path);
        }

        public static void DownloadFile(Uri uri, string path)
        {
            var client = new WebClient();
            client.DownloadFile(uri, path);
        }
    }

    /// <summary>
    /// Encode a set of values into a URL compatible string
    /// </summary>
    public class UrlEncodedDataBuilder
    {
        // All keys and values
        internal HashSet<string> fields = new HashSet<string>();

        /// <summary>
        /// Add a key and value
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(string key, string value)
        {
            // Replace any chars like " " with something like "%20"
            fields.Add($"{Uri.EscapeUriString(key)}={Uri.EscapeUriString(value)}");
        }

        /// <summary>
        /// Remove a key and value
        /// </summary>
        /// <param name="key"></param>
        public void Remove(string key)
        {
            string found = key;
            foreach (var item in fields)
            {
                if (item.StartsWith($"{key}="))
                {
                    found = item;
                    break;
                }
            }

            fields.Remove(found);
        }

        /// <summary>
        /// Create the string
        /// </summary>
        /// <returns></returns>
        public string Build()
        {
            return string.Join("&", fields.ToArray());
        }
    }

    /// <summary>
    /// Method for request
    /// </summary>
    public enum Method
    {
        POST,
        GET,
        HEAD,
        PUT,
        DELETE
    }

    /// <summary>
    /// Response from a NetRequest
    /// </summary>
    public struct NetResponse
    {
        /// <summary>
        /// Content of response
        /// </summary>
        public string Content;

        /// <summary>
        /// HTTP status code
        /// </summary>
        public HttpStatusCode Status;

        /// <summary>
        /// Status description
        /// </summary>
        public string StatusDescription;

        /// <summary>
        /// Content type of response
        /// </summary>
        public string ContentType;

        /// <summary>
        /// Content length
        /// </summary>
        public int ContentLength
        {
            // No setting, only getting
            get
            {
                return Content.Length;
            }
        }
    }
}
