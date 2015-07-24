using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class UserSkillRequest
    {
        internal ushort target;

        public string skillId { get; set; }
    }

    public class UseSkillResponse
    {
        public long skillUpTimestamp;
        
    }

    public class DamageMsg
    {
        public int shipId { get; set; }
        public int damageValue { get; set; }
    }

    public class shipDestroyedMsg
    {
        public int shipId { get; set; }
    }
}
