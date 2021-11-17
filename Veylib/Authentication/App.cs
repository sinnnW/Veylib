using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

using Newtonsoft.Json;

namespace Veylib.Authentication
{
    public class App
    {
        public enum AppState
        {
            Valid,
            Disabled,
            Unknown
        }

        public class AppData
        {
            public AppState State;

            public string Name;
            public string Description;

            public int Id;
            public int OwnerId;

            public bool Disabled;
            public bool Private;
            public bool HWIDRequired;
            public bool SubscriptionsEnabled;
        }

        private static AppData jsonToApp(dynamic json)
        {
            if (json.code == 200)
                return new AppData { State = AppState.Valid, Name = json.extra.name, Description = json.extra.description, Id = json.extra.id, Disabled = json.extra.disabled == 1 ? true : false, HWIDRequired = json.extra.hwidRequired == 1 ? true : false, OwnerId = json.extra.owner_id, Private = (json.extra["private"] == 1 ? true : false), SubscriptionsEnabled = json.extra.subscriptions_enabled == 1 ? true : false };
            else
                return new AppData { State = AppState.Unknown };
        }

        public static string GetVar(string Key)
        {
            // Setup the request
            WebRequest req = WebRequest.Create($"{Shared.APIUrl}/auth/vars/{Shared.AppID}/-1/{Key}");

            // Add headers as a just in case
            req.Headers.Add("Authorization", User.CurrentUser.Token);
            req.Headers.Add("HWID", Shared.HWID);

            // Get the response and parse in from JSON
            string resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(resp);

            // If its not 200, there was an error
            if (json.code != 200)
                throw new ServerError((string)json.message);

            return json.extra.value;
        }

        public static AppData Modify(int appId, dynamic changes)
        {
            if (User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
                throw new UserNotLoggedin();
            else if (!User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.ModifyUsers) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin))
                throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.ModifyUsers);

            var req = Shared.GenerateWR($"auth/app/");
            req.Method = "PATCH";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"app_id\": " + appId + ", \"changes\": " + JsonConvert.SerializeObject(changes) + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToApp(json);
        }

        public static AppData Modify(dynamic changes)
        {
            return Modify(Shared.AppID, changes);
        }

        public static AppData Info(int appId)
        {
            var req = Shared.GenerateWR($"auth/app/{appId}");
            var json = Shared.ReadResponse(req);
            return jsonToApp(json);
        }

        public static AppData Info()
        {
            return Info(Shared.AppID);
        }

        //public static Dictionary<string, string> GetPrivateVars(string AuthToken)
        //{
        //    var user = User.Verify(AuthToken);

        //    if (user.State != User.UserVerificationState.ValidCredentials)
        //        throw new UserNotLoggedin();

        //    // Setup the request
        //    WebRequest req = WebRequest.Create($"{Shared.APIUrl}/auth/app/vars/{Shared.AppID}/private");
        //    req.Headers.Add("Authorization", user.Token);

        //    // Get the response and parse in from JSON
        //    string resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
        //    dynamic json = JsonConvert.DeserializeObject<dynamic>(resp);

        //    // If its not 200, there was an error
        //    if (json.code != 200)
        //        throw new ServerError(json.message);

        //    return Shared.OrganizeProperties(json);
        //}

        //public static Dictionary<string, string> GetPublicVars()
        //{
        //    // Setup the request
        //    WebRequest req = WebRequest.Create($"{Shared.APIUrl}/auth/app/vars/{Shared.AppID}/public");

        //    // Get the response and parse in from JSON
        //    string resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
        //    dynamic json = JsonConvert.DeserializeObject<dynamic>(resp);

        //    // If its not 200, there was an error
        //    if (json.code != 200)
        //        throw new ServerError(json.message);

        //    return Shared.OrganizeProperties(json);
        //}
    }
}
