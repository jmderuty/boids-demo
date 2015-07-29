using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient
{
    public class Simulation
    {
        public Simulation()
        {
            Boid = new Boid();
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
