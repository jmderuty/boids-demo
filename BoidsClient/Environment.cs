using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Environment
    {
        public Environment()
        {
            VisibleShips = new Dictionary<ushort, Ship>();
        }

        public Dictionary<ushort, Ship> VisibleShips { get; private set; }


        public void AddShip(Ship ship)
        {
            VisibleShips.Add(ship.Id, ship);
        }

        public void RemoveShip(ushort id)
        {
            VisibleShips.Remove(id);
        }

        public void UpdateShipLocation(ushort id, float x, float y, float rot)
        {
            Ship ship;
            if (!VisibleShips.TryGetValue(id, out ship))
            {
                ship = new Ship { Id = id };
                VisibleShips.Add(id, ship);
            }
            ship.X = x;
            ship.Y = y;
            ship.Rot = rot;
        }
    }
}
