using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidsClient.Cmd
{
    interface IHandler
    {
        bool IsRunning { get;  }

        void Run();
        Task Start(Stormancer.Client client);

        void Stop();
    }
}
