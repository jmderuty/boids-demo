namespace Server
{
    public class ShipDamageMsg
    {
        public ushort shipId;

        public int pvLost { get; set; }

        public ushort origin { get; set; }

        public string weaponId { get; set; }
    }
}