using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veylib.VeyAPI
{
    public class Invite
    {
        public string Code;
        public Application Application;
        public User CreatedBy;
        public User ClaimedBy;
        public DateTime Expires;
    }
}
