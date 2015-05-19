using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Simulation
    {
        public Simulation(float x, float y, float rot)
        {
            Boid = new Boid(x, y, rot);
            Environment = new Environment();
        }

        public Boid Boid { get; private set; }

        public Environment Environment { get; private set; }

        private DateTime lastRun = DateTime.UtcNow;
        public void Step()
        {
            var dt = (DateTime.UtcNow - lastRun).TotalMilliseconds / 1000;
            if (dt > 0.010)
            {
                lastRun = DateTime.UtcNow;
                Boid.Step((float)dt, Environment);
            }
        }
       
    }
}
