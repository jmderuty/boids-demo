using BoidsClient.Worker;
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
                //ThreadPool.SetMaxThreads(workerThreads: 400, completionPortThreads: 400);
                //ThreadPool.SetMinThreads(workerThreads: 200, completionPortThreads: 200);
                var nbBoids = int.Parse(args[0]);
                var pM = new PeerManager(ConfigurationManager.AppSettings["accountId"], ConfigurationManager.AppSettings["applicationName"],
                    ConfigurationManager.AppSettings["sceneName"]);
                WriteLogs();
                var cts = new CancellationTokenSource();
                pM.SetInstanceCount(nbBoids);
                pM.RunPeers(200, cts.Token);

                Console.Read();
                cts.Cancel();
            }
            catch (Exception ex)
            {
            }
        }

        private static async Task WriteLogs()
        {
            while (true)
            {
                await Task.Delay(60 * 1000);
                var d = DateTime.UtcNow;

                Console.WriteLine("{0} : write: {1}", d, JsonConvert.SerializeObject(Metrics.Instance.GetRepository("write").ComputeMetrics()));
                Console.WriteLine("{0} : send: {1}", d, JsonConvert.SerializeObject(Metrics.Instance.GetRepository("send").ComputeMetrics()));
                Console.WriteLine("{0} : sim: {1}", d, JsonConvert.SerializeObject(Metrics.Instance.GetRepository("sim").ComputeMetrics()));
            }
        }
        private static void Domain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
        }
    }
}
