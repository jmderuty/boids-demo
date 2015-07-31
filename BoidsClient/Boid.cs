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
        public float X { get; set; }
        public float Y { get; set; }
        public float Rot { get; set; }

        public bool CanAttack { get; set; }
        public ShipStatus Status { get; set; }
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
            if (this.Status == ShipStatus.InGame)
            {
                Flock(environment.VisibleShips.Values);

                CheckSpeed();

                Rot += dr;
                var dx = (float)Math.Cos(Rot) * speed * dt;
                var dy = (float)Math.Sin(Rot) * speed * dt;
                X += dx;
                Y += dy;
                if (CanAttack)
                {
                    Fight(environment.VisibleShips.Values);
                }
            }
        }
        private Random _rand = new Random();
        private void Fight(IEnumerable<Ship> ships)
        {
            var q = from weapon in Weapons where isAvailable(weapon) select weapon;
            foreach (var w in q)
            {
                var targets = ships.Where(s => AtRange(s, w.Weapon)).ToArray();
                Ship target = targets.Length > 0 ? targets[_rand.Next(targets.Length)] : null;

                if (target != null)
                {
                    System.Diagnostics.Debug.WriteLine(w.nextFireTry + " " + Clock() + " " + w.Weapon.coolDown);
                    w.nextFireTry = Clock() + w.Weapon.coolDown+200;
                    
                    Fire(target, w.Weapon).ContinueWith(t =>
                    {
                        if (t.Result.error)
                        {
                            //Console.WriteLine("{0} -- ERROR -->{1} : {2}", Id, target.Id, t.Result.errorMsg);
                            w.nextFireTry = Clock()+200;//Make sure that we will retry in more than 100ms.
                        }
                        else
                        {
                            //Console.WriteLine("{0} --  {2}  --> {1}", Id, target.Id, t.Result.success ? ">" : "x");
                           
                            w.nextFireTry = Clock() + w.Weapon.coolDown + 200;
                        }

                    });
                }
            }
        }

        private bool isAvailable(WeaponViewModel weapon)
        {
            return Clock() > weapon.nextFireTry;
        }

        private bool AtRange(Ship ship, Weapon weapon)
        {
            return ship.Status == ShipStatus.InGame && (ship.X - X) * (ship.X - X) + (ship.Y - Y) * (ship.Y - Y) < (weapon.range - 10) * (weapon.range - 10);
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


        public List<WeaponViewModel> Weapons { get; set; }
    }
}
