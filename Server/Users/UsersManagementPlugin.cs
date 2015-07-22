using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using Stormancer.Plugins;
using Stormancer.Server;
using Stormancer;

namespace Server.Users
{
    class UsersManagementPlugin : Stormancer.Plugins.IHostPlugin
    {
        private readonly UserManagementConfig _config;
        private readonly IUserService _userService;
        public UsersManagementPlugin(UserManagementConfig config = null)
        {
            if (config == null)
            {
                config = new UserManagementConfig();
            }
            _config = config;
            _userService = new UserService(config);
        }
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarting += HostStarting;

            ctx.SceneCreating += SceneCreating;
        }
        private void HostStarting(IHost host)
        {
            host.AddSceneTemplate("authenticator", AuthenticatorSceneFactory);
        }

        private void SceneCreating(ISceneHost scene)
        {

            scene.RegisterComponent<IUserService>(() => _userService);
        }

        private void AuthenticatorSceneFactory(ISceneHost scene)
        {
            scene.AddProcedure("login", async p =>
            {

                var authenticationCtx = p.ReadObject<Dictionary<string, string>>();

                foreach (var provider in _config.AuthenticationProviders)
                {

                    if (await provider.Authenticate(authenticationCtx, _userService))
                    {
                        break;
                    }
                }


                //p.SendValue();

            });

            foreach (var provider in _config.AuthenticationProviders)
            {
                provider.AdjustScene(scene);
            }

        }
        private Dictionary<string, string> GetAuthenticateRouteMetadata()
        {
            var result = new Dictionary<string, string>();

            foreach (var provider in _config.AuthenticationProviders)
            {
                provider.AddMetadata(result);
            }

            return result;
        }
    }


    public class AuthenticationResult
    {

    }
}
