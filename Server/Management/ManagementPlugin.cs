using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using Stormancer.Management.Client;
using Stormancer.Plugins;
using Stormancer.Server.Components;

namespace Server.Management
{
    class ManagementPlugin : IHostPlugin
    {
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.SceneCreating += OnSceneCreating;
        }

        private void OnSceneCreating(ISceneHost scene)
        {
            //var lazy = new Lazy<Stormancer.Management.Client.ApplicationClient>(() => {
            //    var environment = scene.GetComponent<IEnvironment>();
            //    environment.GetApplicationInfos();
            //});
        }
    }

    public class ManagementClientAccessor
    {
        
        private ApplicationClient _appClient;
        public ManagementClientAccessor(IEnvironment environment)
        {
            
        }
        public Task<Stormancer.Management.Client.ApplicationClient> GetApplicationClient()
        {
            throw new NotImplementedException();
        }
    }
}
