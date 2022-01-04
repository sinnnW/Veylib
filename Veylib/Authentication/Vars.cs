using System.Text;

namespace Veylib.Authentication
{
    public class Vars
    {
        public enum VarState
        {
            UnknownVar,
            Success,
            ServerError,
            UnknownError
        }

        public class VarData
        {
            public VarState State;
            public string ErrorMessage;

            public string Key;
            public string Value;

            public int ApplicationId;
            public int UserId;

            public bool Private;
        }

        private static VarData jsonToVar(dynamic json)
        {
            if (json.code == 200)
                return new VarData { State = VarState.Success, ApplicationId = json.extra.application_id, UserId = json.extra.user_id, Key = json.extra.key, Value = json.extra.value, Private = ((int)json.extra["private"] == 1 ? true : false)};
            else if (json.code == 400)
            {
                if (json.extra == null)
                    return new VarData { State = VarState.UnknownError, ErrorMessage = json.message };

                VarState err;
                switch (((string)json.extra).ToLower())
                {
                    case "unknown key.":
                        err = VarState.UnknownVar;
                        break;
                    default:
                        err = VarState.ServerError;
                        break;
                }

                return new VarData { State = err, ErrorMessage = (string)json.extra };
            }
            else
                return new VarData { State = VarState.UnknownError };
        }

        #region Getting variables

        /// <summary>
        /// Get a variable
        /// </summary>
        /// <param name="key">Key of the var</param>
        /// <param name="userId">User ID of the var</param>
        /// <returns>The variable</returns>
        public static VarData Get(string key, int userId)
        {
            var req = Shared.GenerateWR($"auth/vars/{Shared.AppID}/{userId}/{key}");
            var json = Shared.ReadResponse(req);

            return jsonToVar(json);
        }

        /// <summary>
        /// Get a variable, attempts to get from current user ID
        /// </summary>
        /// <param name="key">Key of the var</param>
        /// <returns>The variable</returns>
        public static VarData Get(string key)
        {
            //if (User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
            //    throw new User.NotLoggedIn();

            return Get(key, User.CurrentUser.Id);
        }

        #endregion

        #region Setting variables

        /// <summary>
        /// Create or set a variable key
        /// </summary>
        /// <param name="key">Key for the var</param>
        /// <param name="value">The value</param>
        /// <param name="userId">The user ID it will be under</param>
        /// <param name="priv">Is the var private</param>
        /// <returns>The created var</returns>
        /// <exception cref="User.NotLoggedIn">User is not logged in</exception>
        /// <exception cref="User.Permissions.InvalidPermissions">User does not have permission</exception>
        public static VarData Set(string key, string value, int userId, bool priv)
        {
            if (User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
                throw new User.NotLoggedIn();
            else if (!User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.CreateVars))
                throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.CreateVars);

            var req = Shared.GenerateWR($"auth/vars/{Shared.AppID}/{userId}/{key}");
            req.Method = "PUT";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"value\": \"" + value + "\", \"private\": " + priv.ToString().ToLower() + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToVar(json);
        }

        /// <summary>
        /// Create or set a variable key
        /// </summary>
        /// <param name="key">Key for the var</param>
        /// <param name="value">The value</param>
        /// <param name="priv">Is the var private</param>
        /// <returns>The var created</returns>
        /// <exception cref="User.NotLoggedIn">User is not logged in</exception>
        public static VarData Set(string key, string value, bool priv)
        {
            if (User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
                throw new User.NotLoggedIn();

            return Set(key, value, User.CurrentUser.Id, priv);
        }

        #endregion

        #region Deleting variables
        
        /// <summary>
        /// Delete a variable from name
        /// </summary>
        /// <param name="key">Key name</param>
        /// <param name="userId">User ID for var</param>
        /// <exception cref="User.NotLoggedIn">User is not logged in</exception>
        /// <exception cref="User.Permissions.InvalidPermissions">User does not have permission</exception>
        public static void Delete(string key, int userId)
        {
            if (User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
                throw new User.NotLoggedIn();
            else if (!User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.DeleteVars))
                throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.DeleteVars);

            var req = Shared.GenerateWR($"auth/vars/{Shared.AppID}/{userId}/{key}");
            req.Method = "DELETE";

            req.GetResponse();
        }

        /// <summary>
        /// Delete a variable from name
        /// </summary>
        /// <param name="key">Key name</param>
        /// <param name="userId">User ID for var</param>
        /// <exception cref="User.NotLoggedIn">User is not logged in</exception>
        /// <exception cref="User.Permissions.InvalidPermissions">User does not have permission</exception>
        public static void Delete(string key)
        {
            Delete(key, User.CurrentUser.Id);
        }

        #endregion
    }
}
