using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veylib.Authentication
{
    public partial class User
    {
        public class NotLoggedIn : Exception
        {
            public override string Message
            {
                get
                {
                    return "No user is not logged in.";
                }
            }
        }

        public partial class Permissions
        {
            public class InvalidPermissions : Exception
            {
                public InvalidPermissions(Permissions.Flags missingFlags)
                {
                    missing = missingFlags;
                }

                private Permissions.Flags missing;

                public override string Message
                {
                    get
                    {
                        return $"Missing permission: {missing}";
                    }
                }
            }
        }
    }

    public class UserNotLoggedin : Exception
    { }
    public class ServerError : Exception
    {
        public ServerError(string serverReturn)
        {
            servRet = serverReturn == null ? "Unreported error" : servRet;
        }

        private string servRet;
        public override string Message {
            get {
                return servRet;
            }
        }
    }

    public class MissingVariables : Exception
    {
        public MissingVariables(string varName)
        {

        }

        private string var;

        public override string Message
        {
            get
            {
                return $"Missing {var} in configuration!";
            }
        }
    }
}
