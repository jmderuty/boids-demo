using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    class Program: MarshalByRefObject
    {

        static void Main(string[] args)
        {
            try
            {
                var nbBoids = int.Parse(args[0]);
               
                for (int i = 0; i < nbBoids; i++)
                {
                    var name = "peer-" + i;
                    var domain = AppDomain.CreateDomain(name);
                    domain.UnhandledException += Domain_UnhandledException;
                    var proxy = (PeerProxy)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(PeerProxy).FullName);
                    //var proxy = new PeerProxy();
                    proxy.Start(name);
                    Thread.Sleep(1000);
                }

                Console.Read();
            }
            catch (Exception ex)
            {
            }
        }

        private static void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
