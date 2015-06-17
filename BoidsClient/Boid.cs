using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Boid
    {

        public Boid(float x, float y, float rot)
        {
            X = x;
            Y = y;
            Rot = rot;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public float Rot { get; private set; }
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
            foreach (var ship in ships)
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
        }


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

    }
}
