using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using Newtonsoft.Json;
using Veylib.Utilities.Net;

namespace Veylib.VeyAPI
{
    public class User
    {
        public bool Authenticated;
        public int Id;
        public Application Application;
        public string Username;
        public bool Disabled;
        public string DisableReason;
        public string? HWID;
        public string Token;
        public Permissions.Flags Permissions;
        public File[] Files;
        public Variable[] Variables;
        public Invite[] Invites;
        public Invite? Claimed;

        //public static User Get(int id)
        //{
        //    var res = new NetRequest(Core.buildUrl($"auth/user/get/{id}")).Send();

        //    if (res.Status != HttpStatusCode.OK)
        //        throw new Exception(res.Content);

        //    return fill(res.Content);
        //}

        //internal static User fill(dynamic data)
        //{
        //    if (data == null)
        //        throw new ArgumentNullException();
        //    else if (data == "[Circular]")
        //        return null;

        //    data = JsonConvert.DeserializeObject(data);

        //    var user = new User();
        //    user.Authenticated = data.authenticated;
        //    user.Id = data.id;
        //    user.Application = Application.fill(data.application);
        //    user.Username = data.username;
        //    user.Disabled = data.disabled;
        //    user.DisableReason = data.disableReason;
        //    user.HWID = data.hwid;
        //    user.Token = data.token;
        //    user.Permissions = data.permissions
        //}
    }
}
