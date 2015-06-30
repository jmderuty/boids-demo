using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BoidsClient.Cmd;

using System.Threading;
using System.Reactive.Concurrency;

namespace BoidsClient.Worker
{
    public class PeerManager
    {
        private string _accountId;
        private string _app;
        private string _sceneId;
        public PeerManager(string accountId, string app, string sceneId)
        {
            _accountId = accountId;
            _app = app;
            _sceneId = sceneId;
        }
        private List<Peer> _peers = new List<Peer>();
        public int RunningInstances
        {
            get
            {
                return _peers.Count;
            }
        }

        private int i;
        private int _currentTargetInstanceCount;

        public async Task SetInstanceCount(int count)
        {
            if (count < 0)
            {
                throw new ArgumentException("Count must be positive.");
            }

            while (count != RunningInstances)
            {
                if (count < RunningInstances)
                {
                    RemoveInstance();
                    await Task.Delay(1000);
                }

                if (count > RunningInstances)
                {
                    AddInstance();
                    await Task.Delay(1000);
                }
            }



        }

        private void RemoveInstance()
        {
            var peer = _peers.Last();
            _peers.Remove(peer);
            peer.Proxy.Stop();
            //AppDomain.Unload(peer.Domain);
        }

        private async Task AddInstance()
        {
            var name = "peer" + i++;
            //var domain = AppDomain.CreateDomain(name, null, AppDomain.CurrentDomain.BaseDirectory, "", true);
            //var path = domain.BaseDirectory;
            var proxy = new PeerProxy();
            //var proxy = (PeerProxy)domain.CreateInstanceAndUnwrap(typeof(PeerProxy).Assembly.FullName, typeof(PeerProxy).FullName);
            var peer = new Peer { Proxy = proxy };
            _peers.Add(peer);


            proxy.Stopped = () =>
            {
                _peers.Remove(peer);
            };
            await proxy.Start(name, _accountId, _app, _sceneId);
        }

        public void RunPeers(int delay, CancellationToken ct)
        {

            var watch = new Stopwatch();
            
            var disposable = DefaultScheduler.Instance.SchedulePeriodic(TimeSpan.FromMilliseconds(delay), () => {
                Metrics.Instance.GetRepository("period").AddSample(0, watch.ElapsedMilliseconds);
                watch.Restart();
                foreach (var peer in _peers)
                {
                    if (peer.Proxy != null)
                    {
                        peer.Proxy.RunStep();
                    }
                }
                var t = watch.ElapsedMilliseconds;
                var dt = delay - t;
                Metrics.Instance.GetRepository("total_step_duration").AddSample(0, t);
            });

            ct.Register(() => disposable.Dispose());

        }
        private class Peer
        {
            public AppDomain Domain { get; set; }
            public PeerProxy Proxy { get; set; }
        }
    }
}
