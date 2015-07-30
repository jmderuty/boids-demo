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

            var admintest = builder.AdminPlugin("admintest", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("admintest");
            //admintest.Get["/"] = ctx => "helloworld";

            var viewer = builder.AdminPlugin("viewer", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("Viewer");

            var leaderboards = builder.AdminPlugin("leaderboards", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("Leaderboards");
        }
    }
}
