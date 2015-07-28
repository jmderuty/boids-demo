using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Boid
    {

        public Boid()
        {
          
        }
        public ushort Id { get; set; }
        public float X { get;  set; }
        public float Y { get;  set; }
        public float Rot { get;  set; }
        //Vitesse max (m/s);
        private float speed = 15;
        //Vitesse de rotation max (rad/s)
        private float drMax = (float)Math.PI / 32;
        private float dr;

        private float space = 10;

        private float Distance(Ship ship)
        {
            return Distance(ship.X, ship.Y);
        }
        private float Distance(float x, float y)
        {
            return (X - x) * (X - x) + (Y - y) * (Y - y);
        }
        private void Flock(IEnumerable<Ship> ships)
        {
            float dX = 0;
            float dY = 0;
            foreach (var ship in ships.ToArray())
            {
                float distance = Distance(ship);

                if (distance < space)
                {

                    // Create space.
                    dX += X - ship.X;
                    dY += Y - ship.Y;
                }
                else
                {
                    // Flock together.
                    dX += (ship.X - X) * 0.05f;
                    dY += (ship.Y - Y) * 0.05f;
                }

            }
            var centerDistance = Distance(0, 0);

            dX += -X * Math.Abs(X) * 0.05f;
            dY += -Y * Math.Abs(Y) * 0.05f;

            var tr = Math.Atan2(dY, dX);

            dr = (float)(tr - Rot);
            if (dr < -Math.PI)
            {
                dr += 2 * (float)Math.PI;
            }
            if (dr > Math.PI)
            {
                dr -= 2 * (float)Math.PI;
            }
            dr *= 0.1f;

        }

        /// <summary>
        /// Runs a simulation step for the boid
        /// </summary>
        /// <param name="dt">timestep in seconds</param>
        /// <param name="environment">Environment of the boid.</param>
        public void Step(float dt, Environment environment)
        {
            
            Flock(environment.VisibleShips.Values);

            CheckSpeed();

            Rot += dr;
            var dx = (float)Math.Cos(Rot) * speed * dt;
            var dy = (float)Math.Sin(Rot) * speed * dt;
            X += dx;
            Y += dy;

            Fight(environment.VisibleShips.Values);
        }

        private void Fight(IEnumerable<Ship> ships)
        {
            var q = from weapon in Weapons where isAvailable(weapon) select weapon;
           foreach(var w in q)
           {
               var target = ships.FirstOrDefault(s => AtRange(s, w));

               if(target != null)
               {
                   Fire(target, w).ContinueWith(t=>{
                       if(t.IsFaulted)
                       {
                           Console.WriteLine("{0} --FAILED -->{1}", Id, target.Id);
                       }
                       else
                       {
                           Console.WriteLine("{0} --       --> {2}", Id, target.Id);
                       }
                   });
               }
           }
        }

        private bool isAvailable(Weapon weapon)
        {
            return Clock() > weapon.fireTimestamp + weapon.coolDown;
        }

        private bool AtRange(Ship ship, Weapon weapon)
        {
            return (ship.X - X) * (ship.X - X) + (ship.Y - Y) * (ship.Y - Y) < weapon.range * weapon.range;
        }

        public Func<long> Clock;

        public Func<Ship, Weapon, Task<UseSkillResponse>> Fire;

        private void CheckSpeed()
        {
            if (dr > drMax)
            {
                dr = drMax;
            }
            else if (dr < -drMax)
            {
                dr = -drMax;
            }
        }


        public List<Weapon> Weapons { get; set; }
    }
}
