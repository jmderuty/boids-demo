using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    class Program
    {

        static void Main(string[] args)
        {
            var nbBoids = 32;

            for (int i = 0; i < nbBoids; i++)
            {
                var name = "peer-" + i;
                var domain = AppDomain.CreateDomain(name);
                var proxy = (PeerFactory)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "BoidsClient.Cmd.PeerFactory");

                proxy.Start(name);
              
            }

            Console.Read();
        }
    }
}
