using Stormancer;
using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Leaderboards
{
    class LeaderBoardPlugin : IHostPlugin
    {
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.SceneCreating += scene =>
            {
                scene.RegisterComponent<ILeaderboard>(() => new Leaderboard());
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
