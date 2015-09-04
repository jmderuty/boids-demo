using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Models
{
    public class StatusChangedMsg
    {
        public ushort shipId;
        public ShipStatus status;
    }
}
