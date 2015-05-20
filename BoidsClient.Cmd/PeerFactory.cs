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
        public void Start(string name)
        {
            try
            {
                _peer = new Peer(name);

                _peer.Start();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }


        }

        public void Stop()
        {
            _peer.Stop();
        }
    }
}
