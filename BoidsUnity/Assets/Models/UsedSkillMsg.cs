using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public class UsedSkillMsg
    {
        public ushort shipId { get; set; }

        public bool success { get; set; }

        public ushort origin { get; set; }

        public string weaponId { get; set; }

        public long timestamp { get; set;}
    }
}
