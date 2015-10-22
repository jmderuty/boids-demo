using Newtonsoft.Json;
using Stormancer.Authentication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Authentication
{
    public class AuthenticatorService
    {
        #region Config
        private string _authenticatorSceneName = "authenticator";
        public string AuthenticatorSceneName
        {
            get { return _authenticatorSceneName; }
            set { _authenticatorSceneName = value; }
        }

        private string _createUserRoute = "provider.loginpassword.createAccount";
        public string CreateUserRoute
        {
            get { return _createUserRoute; }
            set { _createUserRoute = value; }
        }

        private string _loginRoute = "login";
        public string LoginRoute
        {
            get { return _loginRoute; }
            set { _loginRoute = value; }
        }
        #endregion

        #region fields
        private readonly Client _client;
        private Task<Scene> _authenticatorScene;
        #endregion

        public AuthenticatorService(Client client)
        {
            _client = client;
        }

        /// <summary>
        /// Logs a user in using the Login/Passowrd authentication provider.
        /// </summary>
        /// <param name="login">The user login.</param>
        /// <param name="password">The user password.</param>
        /// <returns>A task returning the scene to be logged in.</returns>
        /// <remarks>The returned scene is not connected yet. In most cases, you will want to add some routes to listen to before connecting to it.</remarks>
        public Task<Scene> Login(string login, string password)
        {
            return Login(new Dictionary<string, string>
            {
                { "login", login },
                {"password", password },
                { "provider", "loginpassword" }
            });
        }

        /// <summary>
        /// Logs a user in using the provided authentication context.
        /// </summary>
        /// <param name="authenticationContext">A key/value dictionary with the values used by the authentication providers on the server.</param>
        /// <returns>A task returning the scene to be logged in.</returns>
        /// <remarks>The returned scene is not connected yet. In most cases, you will want to add some routes to listen to before connecting to it.</remarks>
        public async Task<Scene> Login(Dictionary<string, string> authenticationContext)
        {
            EnsureAuthenticatorSceneExists();

            var scene = await _authenticatorScene;

            var loginResult = await scene.RpcTask<Dictionary<string, string>, LoginResult>(LoginRoute, authenticationContext, Core.PacketPriority.HIGH_PRIORITY);

            if (!loginResult.Success)
            {
                throw new InvalidCredentialException(loginResult.ErrorMsg);
            }

            return await _client.GetScene(loginResult.Token);
        }

        /// <summary>
        /// Creates a new user account using the login/password authentication provider.
        /// </summary>
        /// <typeparam name="T">The type of UserData to store for this user. T must map to a json object: it cannot be a simple string or a numeric type.</typeparam>
        /// <param name="login">The login of the user to create.</param>
        /// <param name="password">The password of the user to create.</param>
        /// <param name="email"></param>
        /// <param name="userData"></param>
        /// <returns></returns>
        /// <remarks>The created user will not be active until a few seconds have passed.</remarks>
        public async Task CreateLoginPasswordAccount<T>(string login, string password, string email, T userData)
        {
            EnsureAuthenticatorSceneExists();

            var scene = await _authenticatorScene;

            if (!scene.RemoteRoutes.Any(r => r.Name == CreateUserRoute))
            {
                throw new InvalidOperationException("User creation is disabled in this application.");
            }

            var createAccountRequest = new CreateAccountRequest
            {
                Login = login,
                Password = password,
                Email = email,
                UserData = JsonConvert.SerializeObject(userData)
            };         



            var createAccountResult = await scene.RpcTask<CreateAccountRequest, LoginResult>(CreateUserRoute, createAccountRequest);

            if (!createAccountResult.Success)
            {
                throw new InvalidOperationException(createAccountResult.ErrorMsg);
            }

            //var tcs = new TaskCompletionSource<bool>();

            //var createAccountObservable = scene.Rpc(CreateUserRoute, s => scene.Host.Serializer().Serialize(createAccountRequest, s));

            //createAccountObservable.Subscribe(packet =>
            //{
            //    Console.WriteLine("packet received!");
            //},exception =>
            //{
            //    Console.WriteLine("An exception occured!");
            //}
            //,()=>
            //{
            //    Console.WriteLine("Create account finished!");
            //});

            //await tcs.Task;
        }

        private void EnsureAuthenticatorSceneExists()
        {
            _authenticatorScene = _authenticatorScene ?? GetAuthenticatorScene();
        }

        private async Task<Scene> GetAuthenticatorScene()
        {
            var result = await _client.GetPublicScene(AuthenticatorSceneName, "");

            await result.Connect();

            return result;
        }

        /// <summary>
        /// Logs the connected user out. 
        /// </summary>
        /// <returns></returns>
        public async Task Logout()
        {
            var scene = await _authenticatorScene;
            _authenticatorScene = null;

            if (scene != null)
            {
                if (scene.Connected)
                {
                    await scene.Disconnect();
                }
            }
        }
    }
}
