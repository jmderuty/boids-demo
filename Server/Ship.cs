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

        public byte[] LastPositionRaw { get; internal set; }
        public DateTime PositionUpdatedOn { get; internal set; }
    }
}