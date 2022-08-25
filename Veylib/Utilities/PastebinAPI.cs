using System;
using System.Collections.Generic;
using System.Net;

using Veylib.Utilities.Net;

/*
 * Access Pastebin's API
 */

namespace Veylib.Utilities
{
    public class PastebinAPI
    {
        // The API key
        internal string key;

        /// <summary>
        /// Allow for access to Pastebin's API
        /// </summary>
        /// <param name="apiKey">Pastebin account API key</param>
        public PastebinAPI(string apiKey)
        {
            key = apiKey;
        }

        /// <summary>
        /// The publicity of the paste
        /// </summary>
        [Flags]
        public enum Publicity
        {
            Public = 0,
            Unlisted = 1,
        }

        /// <summary>
        /// Paste structure
        /// </summary>
        public class Paste
        {
            public Paste() {
                Url = null;
                Publicity = Publicity.Public;
                Format = "";
                Title = "Unlisted";
                Content = "";
            }

            /// <summary>
            /// Url to paste
            /// </summary>
            public string? Url;

            /// <summary>
            /// Paste publiciy
            /// </summary>
            public Publicity Publicity;

            /// <summary>
            /// Syntax highlighting
            /// </summary>
            public string Format;
            
            /// <summary>
            /// Title on pastebin
            /// </summary>
            public string Title;

            /// <summary>
            /// Actual content
            /// </summary>
            public string Content;
        }

        /// <summary>
        /// Create a paste
        /// </summary>
        /// <param name="options">Options</param>
        /// <returns>Created paste</returns>
        /// <exception cref="Exception"></exception>
        public Paste Create(Paste options)
        {
            // Use Veylib net utilities
            var req = new NetRequest("https://pastebin.com/api/api_post.php");
            req.SetMethod(Method.POST);
            req.SetContentType("application/x-www-form-urlencoded");

            // Create a urlencoded body
            var builder = new UrlEncodedDataBuilder();
            builder.Add("api_dev_key", key);
            builder.Add("api_option", "paste");
            builder.Add("api_paste_name", options.Title);
            builder.Add("api_paste_format", options.Format);
            builder.Add("api_paste_private", ((int)options.Publicity).ToString());
            builder.Add("api_paste_code", options.Content);

            // Build and set body
            req.SetContent(builder.Build());

            // Send request
            var resp = req.Send();

            // Validate request response
            if (resp.Status != HttpStatusCode.OK)
                throw new Exception("Failed to create paste");

            // Return.
            options.Url = resp.Content;
            return options;
        }

        /// <summary>
        /// Create a paste
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <returns>Created paste</returns>
        public Paste Create(string title, string content)
        {
            return Create(new Paste { Title = title, Content = content });
        }
        
        /// <summary>
        /// Create a paste
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="publicity"></param>
        /// <returns>Created paste</returns>
        public Paste Create(string title, string content, Publicity publicity)
        {
            return Create(new Paste { Title = title, Content = content, Publicity = publicity });
        }

        /// <summary>
        /// Create a paste
        /// </summary>
        /// <param name="title"></param>
        /// <param name="content"></param>
        /// <param name="format"></param>
        /// <returns>Created paste</returns>
        public Paste Create(string title, string content, string format)
        {
            return Create(new Paste { Title = title, Content = content, Format = format });
        }
    }
}
