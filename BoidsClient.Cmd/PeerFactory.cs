using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{

    public class PeerProxy : MarshalByRefObject
    {
        private Peer _peer;
        public void Start(string name, string accountId, string app, string scene)
        {
            try
            {
                _peer = new Peer(name, accountId, app, scene);

                _peer.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


        }

        public void Stop()
        {
            _peer.Stop();
        }

        internal void Start(string name, object p1, object p2, object p3)
        {
            throw new NotImplementedException();
        }
    }
}
