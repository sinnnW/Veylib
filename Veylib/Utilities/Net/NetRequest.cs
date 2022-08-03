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
        internal WebRequest request;

        public NetRequest(string url)
        {
            request = Create(url);
        }

        public void SetMethod(Method method)
        {
            request.Method = method.ToString();
        }
        
        public void SetHeader(string key, string value)
        {
            request.Headers.Set(key, value);
        }

        public void RemoveHeader(string key)
        {
            request.Headers.Remove(key);
        }

        public void SetContent(dynamic content)
        {
            var bytes = Encoding.ASCII.GetBytes(content);
            request.GetRequestStream().Write(bytes, 0, bytes.Length);
        }

        public void SetContentType(string contentType)
        {
            request.ContentType = contentType;
        }

        public NetResponse Send()
        {
            var nr = new NetResponse();
            
            WebResponse resp;
            try
            {
                resp = request.GetResponse();
            } catch (WebException ex)
            {
                resp = ex.Response;
            }

            nr.Content = new StreamReader(resp.GetResponseStream()).ReadToEnd();
            nr.ContentType = resp.ContentType;
            nr.Status = ((HttpWebResponse)resp).StatusCode;
            nr.StatusDescription = ((HttpWebResponse)resp).StatusDescription;

            return nr;
        }
    }

    public class UrlEncodedDataBuilder
    {
        internal HashSet<string> fields = new HashSet<string>();

        public void Add(string key, string value)
        {
            fields.Add($"{Uri.EscapeUriString(key)}={Uri.EscapeUriString(value)}");
        }

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

        public string Build()
        {
            return string.Join("&", fields.ToArray());
        }
    }

    public enum Method
    {
        POST,
        GET,
        HEAD,
        PUT,
        DELETE
    }

    public struct NetResponse
    {
        public string Content;
        public HttpStatusCode Status;
        public string StatusDescription;
        public string ContentType;
        public int ContentLength
        {
            get
            {
                return Content.Length;
            }
        }
    }
}
