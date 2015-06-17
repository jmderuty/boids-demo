using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using BoidsClient.Cmd;
using Newtonsoft.Json;

namespace BoidsClient.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("BoidsClient.Worker is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {

            // Définir le nombre maximum de connexions simultanées
            ServicePointManager.DefaultConnectionLimit = 12;

            // Pour plus d'informations sur la gestion des modifications de configuration
            // consultez la rubrique MSDN à l'adresse http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("BoidsClient.Worker has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("BoidsClient.Worker is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("BoidsClient.Worker has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            var repo = new ConfigurationRepository();
            var target = RoleEnvironment.GetConfigurationSettingValue("Stormancer.Target").Split('/');
            var peersManager = new PeerManager(target[0], target[1], target[2]);
            var _ = Task.Run(() => WriteLogs());
            _ = Task.Run(() => peersManager.RunPeers(200, cancellationToken));
            
            while (!cancellationToken.IsCancellationRequested)
            {

               
                try
                {
                    var peersCount = await repo.GetTargetInstancesCount();
                    await peersManager.SetInstanceCount(peersCount);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
                await Task.Delay(1000);

            }
        }

        private static async Task WriteLogs()
        {
            while (true)
            {
                await Task.Delay(10 * 1000);
                var d = DateTime.UtcNow;
                var m = Metrics.Instance.GetRepository("total_step_duration").ComputeMetrics();
                Trace.TraceInformation("total_step_duration: {0}", JsonConvert.SerializeObject(m));
                Trace.TraceInformation("Write              : {0}",JsonConvert.SerializeObject(Metrics.Instance.GetRepository("write").ComputeMetrics()));
                Trace.TraceInformation("Send               : {0}",  JsonConvert.SerializeObject(Metrics.Instance.GetRepository("send").ComputeMetrics()));
                Trace.TraceInformation("Sim                : {0}", JsonConvert.SerializeObject(Metrics.Instance.GetRepository("sim").ComputeMetrics()));
            }
        }
    }
}
