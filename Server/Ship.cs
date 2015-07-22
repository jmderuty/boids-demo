using System;

namespace Server
{
    internal class Ship
    {
        public Player player;
        public ushort id;

        public float x;
        public float y;

        public float rot;

        public int currentPv;
        public int maxPv;

        public ushort team;

        public Weapon[] weapons { get; set;
        }
        public byte[] LastPositionRaw { get; internal set; }
        public DateTime PositionUpdatedOn { get; internal set; }

        public ShipStatus Status { get; set; }
    }


    public enum ShipStatus
    {
        Waiting,
        Game,
        Dead,
        GameComplete
    }
}