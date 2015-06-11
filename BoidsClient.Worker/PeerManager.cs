using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BoidsClient.Cmd;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace BoidsClient.Worker
{
    public class PeerManager
    {
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
            if (count != _currentTargetInstanceCount)
            {
                _currentTargetInstanceCount = count;

                while (_currentTargetInstanceCount != RunningInstances)
                {
                    if (_currentTargetInstanceCount < RunningInstances)
                    {
                        RemoveInstance();

                    }

                    if (_currentTargetInstanceCount > RunningInstances)
                    {
                        AddInstance();
                        await Task.Delay(1000);
                    }
                }

            }

        }

        private void RemoveInstance()
        {
            var peer = _peers.Last();
            _peers.Remove(peer);
            peer.Proxy.Stop();
            AppDomain.Unload(peer.Domain);
        }

        private void AddInstance()
        {
            var name = "peer" + i++;
            //var domain = AppDomain.CreateDomain(name, null, AppDomain.CurrentDomain.BaseDirectory, "", true);
            //var path = domain.BaseDirectory;
            var proxy = new PeerProxy();
            //var proxy = (PeerProxy)domain.CreateInstanceAndUnwrap(typeof(PeerProxy).Assembly.FullName, typeof(PeerProxy).FullName);
            var peer = new Peer { Proxy = proxy };
            _peers.Add(peer);
            var target = RoleEnvironment.GetConfigurationSettingValue("Stormancer.Target").Split('/');
            Trace.TraceInformation("Starting client instance connected to : " + target);
            proxy.Stopped = () => {
                _peers.Remove(peer);
            };
            proxy.Start(name, target[0], target[1], target[2]);
        }



        private class Peer
        {
            public AppDomain Domain { get; set; }
            public PeerProxy Proxy { get; set; }
        }
    }
}
