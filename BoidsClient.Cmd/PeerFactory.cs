using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{

    public class PeerProxy : MarshalByRefObject
    {
        private  Peer _peer;
        public void Start(string name)
        {
            _peer = new Peer(name);
            _peer.Start();

        }

        public void Stop()
        {
            _peer.Stop();
        }
    }
}
