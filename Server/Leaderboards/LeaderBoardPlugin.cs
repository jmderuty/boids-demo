using Stormancer;
using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server.Database;

namespace Server.Leaderboards
{
    class LeaderBoardPlugin : IHostPlugin
    {
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.SceneCreating += scene =>
            {
                scene.RegisterComponent<ILeaderboardsService>(() => new LeaderboardsService(scene.GetComponent<IESClientFactory>()));
            };
        }
    }

    public class Startup
    {
        public void Run(IAppBuilder builder)
        {
            builder.AddPlugin(new LeaderBoardPlugin());
        }
    }
}
