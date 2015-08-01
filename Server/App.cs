using Stormancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Users;

namespace Server
{
    public class App
    {
        public void Run(IAppBuilder builder)
        {
            builder.AddPlugin(new UsersManagementPlugin());
            builder.AddGameScene();

            var admintest = builder.AdminPlugin("admintest", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("admintest");
            //admintest.Get["/"] = ctx => "helloworld";

            var viewer = builder.AdminPlugin("viewer", Stormancer.Server.Admin.AdminPluginHostVersion.V0_1).Name("Viewer");
        }
    }
}
