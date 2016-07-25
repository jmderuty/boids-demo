using Stormancer.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Authentication
{
    public class AuthenticationPlugin : IClientPlugin
    {
        public void Build(PluginBuildContext ctx)
        {
            ctx.BuildingClientResolver += RegisterAuthenticationService;
        }

        private void RegisterAuthenticationService(IDependencyBuilder client)
        {
           

            client.Register<AuthenticatorService>().AsSelf().Singleton();
        }
    }
}
