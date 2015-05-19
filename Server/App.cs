using Stormancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class App
    {
        public void Run(IAppBuilder builder)
        {
            builder.AddGameScene();             
        }
    }
}
