using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    class Program : MarshalByRefObject
    {

        static void Main(string[] args)
        {
            try
            {
                ThreadPool.SetMaxThreads(workerThreads: 400, completionPortThreads: 400);
                ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);
                var nbBoids = int.Parse(args[0]);

                WriteLogs();
                for (int i = 0; i < nbBoids; i++)
                {
                    var name = "peer-" + i;
                    //var domain = AppDomain.CreateDomain(name);
                    //domain.UnhandledException += Domain_UnhandledException;
                    //var proxy = (PeerProxy)domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, typeof(PeerProxy).FullName);
                    var proxy = new PeerProxy();
                     proxy.Start(name, ConfigurationManager.AppSettings["accountId"], ConfigurationManager.AppSettings["applicationName"],
                    ConfigurationManager.AppSettings["sceneName"]);

                    Thread.Sleep(1000);
                }

                Console.Read();
            }
            catch (Exception ex)
            {
            }
        }
        private static async Task WriteLogs()
        {
            while(true)
            {
                await Task.Delay(10 * 1000);
                var d = DateTime.UtcNow;
                var m = Metrics.Instance.GetRepository("expected_intervals").ComputeMetrics();
                Console.WriteLine("{0} : Expected: {1}", d, JsonConvert.SerializeObject(m));
                Console.WriteLine("{0} : Found: {1}", d, JsonConvert.SerializeObject(Metrics.Instance.GetRepository("found_intervals").ComputeMetrics()));
            }
        }
        private static void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
