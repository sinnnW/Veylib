using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Veylib.Authentication
{
    public static partial class Subscription
    {
        public enum SubscriptionState
        {
            NotSubscribed,
            Subscribed,
            SubscriptionExpired,
            ServerError,
            UnknownError
        }

        public class SubscriptionData
        { 
            public SubscriptionState State;
            public string ErrorMessage;

            public int ApplicationId;
            public int UserId;
            public long SubscriptionExpires;
        }

        private static SubscriptionData jsonToSub(dynamic json)
        {
            if (json.code == 200)
                return new SubscriptionData { State = SubscriptionState.Subscribed, ApplicationId = json.extra.application_id, UserId = json.extra.user_id, SubscriptionExpires = json.extra.subscription_expires };
            else if (json.code == 400)
            {
                if (json.extra == null)
                    return new SubscriptionData { State = SubscriptionState.UnknownError, ErrorMessage = json.message };

                SubscriptionState err;
                switch(((string)json.extra).ToLower())
                {
                    case "unknown subscription.":
                        err = SubscriptionState.NotSubscribed;
                        break;
                    case "subscription expired.":
                        err = SubscriptionState.SubscriptionExpired;
                        break;
                    default:
                        err = SubscriptionState.ServerError;
                        break;
                }

                return new SubscriptionData { State = err, ErrorMessage = (string)json.extra };
            }
            else
                return new SubscriptionData { State = SubscriptionState.UnknownError };
        }

        /// <summary>
        /// Get information on a subscription
        /// </summary>
        public static SubscriptionData Info()
        {
            return Info(User.CurrentUser.Id);
        }

        /// <summary>
        /// Get information on a user's subscription
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static SubscriptionData Info(int userId)
        {
            if (!User.IsLoggedIn) throw new User.NotLoggedIn();
            else if (User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.CreateSubscriptions) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin)) throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.CreateSubscriptions);

            var req = Shared.GenerateWR($"auth/subscription/{Shared.AppID}/{userId}");
            var json = Shared.ReadResponse(req);

            return jsonToSub(json);
        }

        /// <summary>
        /// Create a subscription
        /// </summary>
        public static SubscriptionData Create(int userId, long expiresAt)
        {
            if (!User.IsLoggedIn) throw new User.NotLoggedIn();
            else if (!User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.CreateSubscriptions) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin)) throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.CreateSubscriptions);

            var req = Shared.GenerateWR($"auth/subscription/{Shared.AppID}/{userId}");
            req.Method = "PUT";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"expires_at\": " + expiresAt + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToSub(json);
        }

        public static SubscriptionData Create(int userId, DateTime expiresAt)
        {
            return Create(userId, General.FromDateTime(expiresAt));
        }

        /// <summary>
        /// Delete a subscription
        /// </summary>
        public static void Delete(int userId)
        {
            if (!User.IsLoggedIn) throw new User.NotLoggedIn();

            var req = Shared.GenerateWR($"auth/subscription/{Shared.AppID}/${userId}");
            req.Method = "DELETE";

            req.GetResponse();
        }

        /// <summary>
        /// Extend a subscription for <param name="seconds"></param> more seconds
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="seconds"></param>
        public static SubscriptionData Extend(int userId, long seconds)
        {
            if (!User.IsLoggedIn) throw new User.NotLoggedIn();
            if (User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.ModifySubscriptions) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin)) throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.ModifySubscriptions);

            var sub = Info(userId);

            long expires = seconds == 0 ? 0 : sub.SubscriptionExpires + seconds; 
            var req = Shared.GenerateWR($"auth/subscription/{Shared.AppID}/{userId}");
            req.Method = "PATCH";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"changes\": { \"subscription_expires\": " + expires + " } }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToSub(json);
        }

        /// <summary>
        /// Extend a subscription until the new DateTime
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="time">When to extend it until</param>
        public static SubscriptionData Extend(int userId, DateTime time)
        {
            // Only allow for extensions
            return Extend(userId, Math.Max(1, General.FromDateTime(time) - General.EpochTime));
        }

        public static SubscriptionData ExtendForever(int userId)
        {
            return Extend(userId, 0);
        }
    }
}
