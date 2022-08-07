using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veylib.VeyAPI
{
    public class Application
    {
        public int Id;
        public string Name;
        public string Description;
        public bool Disabled;
        public string DisableReason;
        public bool AllowUserSelfDeletion;
        public bool PublicSubscriptions;
        public bool MultipleSubscriptions;
        public bool UsersCanCreateFiles;
        public bool InviteOnly;
        public User Owner;
    }
}
