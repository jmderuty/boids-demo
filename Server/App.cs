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

            var admin = builder.AdminPlugin("Viewer", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("Viewer");
            //admin.Get["/"] = ctx => "helloworld";
        }
    }
}
