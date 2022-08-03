using System;
using System.Collections.Generic;
using System.Net;

using Veylib.Utilities.Net;

namespace Veylib.Utilities
{
    public class PastebinAPI
    {
        internal string key;
        public PastebinAPI(string apiKey)
        {
            key = apiKey;
        }

        [Flags]
        public enum Publicity
        {
            Public = 0,
            Unlisted = 1,
        }

        public static class Format
        {
            public static readonly string PHP = "php";
            public static readonly string CSharp = "csharp";
        }

        public struct Paste
        {
            public Paste() {
                Url = null;
                Publicity = Publicity.Public;
                Format = "";
                Title = "Unlisted";
                Content = "";
            }

            public string? Url;
            public Publicity Publicity;
            public string Format;
            public string Title;
            public string Content;
        }

        public Paste Create(Paste options)
        {
            var req = new NetRequest("https://pastebin.com/api/api_post.php");
            req.SetMethod(Method.POST);
            req.SetContentType("application/x-www-form-urlencoded");

            var builder = new UrlEncodedDataBuilder();
            builder.Add("api_dev_key", key);
            builder.Add("api_option", "paste");
            builder.Add("api_paste_name", options.Title);
            builder.Add("api_paste_format", options.Format);
            builder.Add("api_paste_private", ((int)options.Publicity).ToString());
            builder.Add("api_paste_code", options.Content);

            req.SetContent(builder.Build());

            var resp = req.Send();

            if (resp.Status != HttpStatusCode.OK)
                throw new Exception("Failed to create paste");

            options.Url = resp.Content;
            return options;
        }

        public Paste Create(string title, string content)
        {
            return Create(new Paste { Title = title, Content = content });
        }

        public Paste Create(string title, string content, Publicity publicity)
        {
            return Create(new Paste { Title = title, Content = content, Publicity = publicity });
        }

        public Paste Create(string title, string content, string format)
        {
            return Create(new Paste { Title = title, Content = content, Format = format });
        }
    }
}
