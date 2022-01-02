using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Dynamic;

namespace Veylib.Authentication
{
    public static partial class User
    {
        public static UserData CurrentUser;

        public class UserData
        {
            public UserVerificationState State;
            public string ErrorMessage;
            public int Id;
            public string Username;
            public string Token;
            public Permissions.Flags Permissions;
            public int ApplicationId;
            public bool Disabled;

            public override string ToString()
            {
                return JsonConvert.SerializeObject(this);
            }
        }

        public enum UserVerificationState
        {
            ValidCredentials,
            InvalidCredentials,
            ApplicationDisabled,
            AccountDisabled,
            ServerError,
            UnknownError,
            InvalidHWID,
            UnknownUser,
            UsernameTaken
        }

        internal class verifyPayload
        {
            public verifyPayload(string Username, string Password)
            {
                username = Username;
                password = Password;
            }

            public string username;
            public string password;
            public int app_id = Shared.AppID;
        }

        private static UserData jsonToUser(dynamic json)
        {
            Debug.WriteLine((string)JsonConvert.SerializeObject(json));

            if (json.code == 200)
            {
                var data = new UserData { State = UserVerificationState.ValidCredentials, Id = json.extra.id, Username = json.extra.username, Token = json.extra.token, Permissions = (Permissions.Flags)(int)json.extra.permissions, ApplicationId = json.extra.application_id, Disabled = (json.extra.disabled == 1 ? true : false) };
                return data;
            }
            else if (json.code == 400)
            {
                UserVerificationState err;

                if ((string)json.extra == "{}")
                    return new UserData { State = UserVerificationState.UnknownError, ErrorMessage = json.message };

                switch (((string)json.extra).ToLower())
                {
                    case "invalid credentials were provided.":
                        err = UserVerificationState.InvalidCredentials;
                        break;
                    case "account is disabled.":
                        err = UserVerificationState.AccountDisabled;
                        break;
                    case "application is disabled.":
                        err = UserVerificationState.ApplicationDisabled;
                        break;
                    case "hardware id is invalid.":
                        err = UserVerificationState.InvalidHWID;
                        break;
                    case "unknown user.":
                        err = UserVerificationState.UnknownUser;
                        break;
                    case "username is already taken.":
                        err = UserVerificationState.UsernameTaken;
                        break;
                    default:
                        err = UserVerificationState.ServerError;
                        break;
                }

                return new UserData { State = err, ErrorMessage = json.extra };
            }
            else
                return new UserData { State = UserVerificationState.UnknownError };
        }

        public static bool IsLoggedIn
        {
            get
            {
                return CurrentUser != null && CurrentUser.State == UserVerificationState.ValidCredentials ? true : false;
            }
        }

        /// <summary>
        /// Verify the Username and Password, or their auth token.
        /// </summary>
        /// <param name="Username">The username of the account</param>
        /// <param name="Password">The password of the account</param>
        /// <param name="AuthToken">[Optional] Username and password OR account token</param>
        /// <returns>UserData class</returns>
        public static UserData Verify(string Username, string Password, string AuthToken)
        {
            if (Shared.APIUrl == null)
                throw new MissingVariables("APIUrl");
            else if (Shared.AppID == -1)
                throw new MissingVariables("AppID");

            WebRequest req = WebRequest.Create($"{Shared.APIUrl}/auth/user/");
            req.Method = "POST";
            req.ContentType = "application/json";

            if (AuthToken != null)
                req.Headers.Add("Authorization", AuthToken);

            // Always add HWID header
            req.Headers.Add("HWID", Shared.HWID);

            if (AuthToken == null)
            {
                byte[] body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new verifyPayload(Username, Password)));
                req.GetRequestStream().Write(body, 0, body.Length);
            }

            var resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
            var json = JsonConvert.DeserializeObject<dynamic>(resp);

            var usr = jsonToUser(json);
            CurrentUser = usr;
            return usr;
        }

        /// <summary>
        /// Verify the Username and Password
        /// </summary>
        /// <param name="Username">The username of the account</param>
        /// <param name="Password">The password of the account</param>
        /// <returns>UserData class</returns>
        public static UserData Verify(string Username, string Password)
        {
            return Verify(Username, Password, null);
        }

        /// <summary>
        /// Verify the account token
        /// </summary>
        /// <param name="Token">The Token of the account</param>
        /// <returns>UserData class</returns>
        public static UserData Verify(string Token)
        {
            return Verify(null, null, Token);
        }

        /// <summary>
        /// Create a user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="permissions">Permissions for the account</param>
        /// <returns>User created</returns>
        public static UserData CreateUser(string username, string password, Permissions.Flags permissions = 0, string authToken = null)
        {
            var req = Shared.GenerateWR("auth/user");
            req.Method = "PUT";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"username\": \"" + username + "\", \"password\": \"" + password + "\", \"app_id\": " + Shared.AppID + ", \"permissions\": " + (int)permissions + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
            var json = JsonConvert.DeserializeObject<dynamic>(resp);

            Debug.WriteLine(resp);
            return jsonToUser(json);
        }

        /// <summary>
        /// Create a user with default permissions
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>User created</returns>
        public static UserData CreateUser(string username, string password)
        {
            // Create with no permissions
            return CreateUser(username, password);
        }

        /// <summary>
        /// Create a user with an invite code
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="inviteCode"></param>
        /// <returns>User created</returns>
        public static UserData CreateUser(string username, string password, string inviteCode)
        {
            return CreateUser(username, password, 0, inviteCode);
        }

        /// <summary>
        /// Get information on a user's ID
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User</returns>
        public static UserData Info(int userId)
        {
            var req = Shared.GenerateWR($"auth/user/{userId}");
            var json = Shared.ReadResponse(req);
            return jsonToUser(json);
        }

        /// <summary>
        /// Get information on a username
        /// </summary>
        /// <param name="username"></param>
        /// <returns>User</returns>
        public static UserData Info(string username)
        {
            var req = Shared.GenerateWR($"auth/user/{username}/{Shared.AppID}");
            var json = Shared.ReadResponse(req);

            return jsonToUser(json);
        }

        /// <summary>
        /// Ban a user's ID
        /// </summary>
        /// <param name="userId"></param>
        public static UserData Ban(int userId)
        {
            dynamic changes = new ExpandoObject();
            changes.disabled = true;
            return Modify(userId, changes);
        }

        public static UserData Unban(int userId)
        {
            dynamic changes = new ExpandoObject();
            changes.disabled = false;
            return Modify(userId, changes);
        }

        public static UserData Modify(int userId, dynamic changes)
        {
            if (CurrentUser.State != UserVerificationState.ValidCredentials)
                throw new UserNotLoggedin();
            else if (!CurrentUser.Permissions.HasFlag(Permissions.Flags.ModifyUsers) && !CurrentUser.Permissions.HasFlag(Permissions.Flags.Admin))
                throw new Permissions.InvalidPermissions(Permissions.Flags.ModifyUsers);

            var req = Shared.GenerateWR($"auth/user/{userId}");
            req.Method = "PATCH";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"app_id\": " + Shared.AppID + ", \"changes\": " + JsonConvert.SerializeObject(changes) + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToUser(json);
        }

        /// <summary>
        /// Get a variable for the user
        /// </summary>
        /// <param name="Key">Variable key</param>
        /// <returns>Variable value</returns>
        public static string GetVar(string Key)
        {
            if (CurrentUser.State != UserVerificationState.ValidCredentials)
                throw new UserNotLoggedin();

            // Setup the request
            WebRequest req = WebRequest.Create($"{Shared.APIUrl}/auth/vars/{Shared.AppID}/{CurrentUser.Id}/{Key}");

            // Add headers as a just in case
            req.Headers.Add("Authorization", CurrentUser.Token);
            req.Headers.Add("HWID", Shared.HWID);

            // Get the response and parse in from JSON
            string resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
            dynamic json = JsonConvert.DeserializeObject<dynamic>(resp);

            // If its not 200, there was an error
            if (json.code != 200)
                throw new ServerError((string)json.message);

            return json.extra.value;
        }

        public static UserData ResetHWID(int userId)
        {
            dynamic changes = new ExpandoObject();
            changes.hwid = null;

            return Modify(userId, changes);
        }

        public static partial class Permissions
        {
            [Flags]
            public enum Flags
            {
                User = 0,
                Admin = 1,
                CreateApplications = 2,

                // User specific permissions
                CreateUsrs = 4,
                DeleteUsers = 8,
                ModifyUsers = 16,

                // Var specific permission
                CreateVars = 32,
                DeleteVars = 64,
                ModifyVars = 128,
                ViewPrivateVars = 256,

                // Invite specific permissions
                CreateInvites = 512,
                DeleteInvites = 1024,
                ModifyInvites = 2048,

                // File specific permissions
                UploadFiles = 4096,
                DeleteFiles = 8192,
                ViewPrivateFiles = 16384,

                BypassHWIDCheck = 32768,

                // Subscription based permissions
                CreateSubscriptions = 65536,
                DeleteSubscriptions = 131072,
                ModifySubscriptions = 262144,
            }

            /// <summary>
            /// Get a user's permissions
            /// </summary>
            /// <param name="userId"></param>
            /// <returns>User's permissions</returns>
            public static Flags Get(int userId)
            {
                var req = Shared.GenerateWR($"auth/user/{userId}");
                var resp = new StreamReader(req.GetResponse().GetResponseStream()).ReadToEnd();
                var json = JsonConvert.DeserializeObject<dynamic>(resp);
                return (Flags)(int)json.extras.permissions;
            }

            /// <summary>
            /// Get current user's permissions
            /// </summary>
            /// <returns>User permissions</returns>
            /// <exception cref="NotLoggedIn">User is not logged in</exception>
            public static Flags Get()
            {
                if (CurrentUser.State != UserVerificationState.ValidCredentials)
                    throw new NotLoggedIn();

                return Get(CurrentUser.Id);
            }

            /// <summary>
            /// Set a user's permissions
            /// </summary>
            /// <param name="userId"></param>
            /// <param name="permissions"></param>
            public static Flags Set(int userId, Flags permissions)
            {
                if (CurrentUser.State != UserVerificationState.ValidCredentials)
                    throw new NotLoggedIn();
                if (!CurrentUser.Permissions.HasFlag(Flags.ModifyUsers))
                    throw new InvalidPermissions(Flags.ModifyUsers);

                dynamic changes = new ExpandoObject();
                changes.permissions = (int)permissions;

                return (Flags)Modify(userId, changes).Permissions;
            }
        }
    }
}
