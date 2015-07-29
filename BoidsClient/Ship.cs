using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Ship
    {
        public ushort Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Rot { get; set; }

        public ushort team { get; set; }

        public Weapon[] Weapons { get; set; }

        public ushort Team { get; set; }

        public ShipStatus Status { get; set; }
    }


    
}
namespace  Server
{
    public enum ShipStatus
    {
        Waiting,
        InGame,
        Dead,
        GameComplete
    }
}
