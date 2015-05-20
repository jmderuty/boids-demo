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
            var peersManager = new PeerManager();
            // TODO: Remplacez le texte suivant par votre propre logique.
            while (!cancellationToken.IsCancellationRequested)
            {

                Trace.TraceInformation("Working");
                try
                {
                    var peersCount = await repo.GetTargetInstancesCount();
                    peersManager.SetInstanceCount(peersCount);
                }
                catch(Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
                await Task.Delay(5000);
            }
        }
    }
}
