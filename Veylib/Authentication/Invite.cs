using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using Newtonsoft.Json;

namespace Veylib.Authentication
{
    public static class Invite
    {
        public enum InviteState
        {
            ValidInvite,
            InviteClaimed,
            UnknownInvite,
            ServerError,
            UnknownError
        }

        public class InviteData
        {
            public InviteState State;

            public string Code;
            public long ExpiresAt;
            public int OwnerId;
            public int ClaimedBy; // -1 signifies that it has not been claimed
        }

        private static InviteData jsonToInvite(dynamic json, bool forceSuccess = false)
        {
            if (forceSuccess)
                json.extra = json;

            if (json.code == 200 || forceSuccess)
            {
                return new InviteData { Code = json.extra.invite_code, ExpiresAt = (long)json.extra.expires, OwnerId = (int)json.extra.owner_id, ClaimedBy = (int)(json.extra.claimed_by == null ? -1 :  json.extra.claimed_by) };
            } else if (json.code == 400)
            {
                switch (((string)json.extra).ToLower())
                {
                    case "invalid invite.":
                        return new InviteData { State = InviteState.UnknownInvite };
                    case "invite has already been claimed.":
                        return new InviteData { State = InviteState.InviteClaimed };
                    default:
                        return new InviteData { State = InviteState.ServerError};
                }
            }
            else
            {
                return new InviteData { State = InviteState.UnknownError };
            }
        }

        private static dynamic getRaw(string code, int userId = -1, string method = "get")
        {
            if (User.CurrentUser == null || User.CurrentUser.State != User.UserVerificationState.ValidCredentials)
                throw new User.NotLoggedIn();
            else if (userId == -1)
                userId = User.CurrentUser.Id;

            var req = Shared.GenerateWR($"auth/invite/{code}");
            req.Method = method;

            if (method != "get")
            {
                req.ContentType = "application/json";
                byte[] body = Encoding.UTF8.GetBytes("{ \"app_id\": " + Shared.AppID + ", \"user_id\": " + userId + " }");
                req.GetRequestStream().Write(body, 0, body.Length);
            }

            var json = Shared.ReadResponse(req);

            return json;
        }

        /// <summary>
        /// Get info for an invite code
        /// </summary>
        /// <param name="code">Invite code</param>
        /// <returns>Invite</returns>
        public static InviteData Get(string code)
        {
            return jsonToInvite(getRaw(code));
        }

        /// <summary>
        /// Get all invites for a user account
        /// </summary>
        /// <returns>User invites</returns>
        public static List<InviteData> GetAll()
        {
            var lid = new List<InviteData>();

            var invites = getRaw("", -1, "POST");
            if (invites == null) return lid;
            string inv = JsonConvert.SerializeObject(invites);
            Debug.WriteLine("Invite: " + inv);

            for (var x = 0; x < invites.extra.Count; x++)
                lid.Add(jsonToInvite(invites.extra[x], true));

            return lid;
        }

        /// <summary>
        /// Get all invites for a user account
        /// </summary>
        /// <param name="userId"></param>
        /// <returns>User invites</returns>
        public static List<InviteData> GetAll(int userId)
        {
            var lid = new List<InviteData>();

            var invites = getRaw("", userId, "POST");
            if (invites == null) return lid;

            for (var x = 0; x < invites.extra.Count; x++)
                lid.Add(jsonToInvite(invites[x]));

            return lid;
        }

        /// <summary>
        /// Create an invite to the application, -1 = never expires
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="expiresAt"></param>
        /// <returns>Invite created</returns>
        public static InviteData Create(int userId, long expiresAt)
        {
            if (!User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.Admin) && !User.CurrentUser.Permissions.HasFlag(User.Permissions.Flags.CreateInvites))
                throw new User.Permissions.InvalidPermissions(User.Permissions.Flags.CreateInvites);

            var req = Shared.GenerateWR($"auth/invite/{Shared.AppID}");
            req.Method = "PUT";
            req.ContentType = "application/json";

            byte[] body = Encoding.UTF8.GetBytes("{ \"user_id\": " + userId + ", \"expires_at\": " + expiresAt + " }");
            Debug.WriteLine("{ \"user_id\": " + userId + ", \"expires_at\": " + expiresAt + " }");
            req.GetRequestStream().Write(body, 0, body.Length);

            var json = Shared.ReadResponse(req);
            return jsonToInvite(json);
        }

        public static InviteData Create(int userId)
        {
            return Create(userId, 0);
        }

        public static InviteData Create(long expiresAt)
        {
            if (User.CurrentUser == null) throw new User.NotLoggedIn();
            return Create(User.CurrentUser.Id, expiresAt);
        }

        public static InviteData Create()
        {
            if (User.CurrentUser == null) throw new User.NotLoggedIn();
            return Create(User.CurrentUser.Id, 0);
        }
    }
}
