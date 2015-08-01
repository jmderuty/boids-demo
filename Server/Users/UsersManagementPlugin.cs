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
        
        public UsersManagementPlugin(UserManagementConfig config = null)
        {
            if (config == null)
            {
                config = new UserManagementConfig();
            }
            _config = config;
            
        }
        public void Build(HostPluginBuildContext ctx)
        {
            ctx.HostStarting += HostStarting;

        }
        private void HostStarting(IHost host)
        {
            host.AddSceneTemplate("authenticator", AuthenticatorSceneFactory);
            host.DependencyResolver.Register<UserManagementConfig>(_config);
        }



        private void AuthenticatorSceneFactory(ISceneHost scene)
        {
            scene.AddProcedure("login", async p =>
            {
                var accessor = scene.DependencyResolver.Resolve<Management.ManagementClientAccessor>();
                var authenticationCtx = p.ReadObject<Dictionary<string, string>>();
                var result = new AuthenticationResult();
                var userService = scene.DependencyResolver.Resolve<IUserService>();
                string userId;
                try
                {
                    foreach (var provider in _config.AuthenticationProviders)
                    {
                        userId = await provider.Authenticate(authenticationCtx, userService);
                        if (!string.IsNullOrEmpty(userId))
                        {
                            result.Success = true;
                            var client = await accessor.GetApplicationClient();
                            result.Token = await client.CreateConnectionToken(_config.SceneIdRedirect, userId);
                            userService.SetUid(p.RemotePeer, userId);
                            break;
                        }
                    }
                    if (!result.Success)
                    {
                        result.ErrorMsg = "No authentication provider able to handle these credentials were found.";
                    }
                }
                catch (ClientException ex)
                {
                    result.ErrorMsg = ex.Message;
                }

                p.SendValue(result);




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
        public bool Success { get; set; }

        public string Token { get; set; }

        public string ErrorMsg { get; set; }
    }


}
