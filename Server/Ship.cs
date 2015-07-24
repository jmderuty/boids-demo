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

        public Weapon[] weapons { get; set; }
     
        public long PositionUpdatedOn { get; internal set; }

        public long lastStatusUpdate { get; set; }

        public void UpdateStatus(ShipStatus newStatus, long timestamp)
        {
            Status = newStatus;
            lastStatusUpdate = timestamp;
        }
        public ShipStatus Status { get; set; }
    }


    public enum ShipStatus
    {
        Waiting,
        InGame,
        Dead,
        GameComplete
    }
}