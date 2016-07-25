using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer;
using System.Reactive.Linq;
using Newtonsoft.Json.Linq;
using MsgPack.Serialization;

namespace BoidsClient.Cmd
{
    class AuthenticatorClient 
    {
        private Scene _scene;
        public bool IsRunning
        {
            get;
            private set;
        }

  
        public Task<string> GetPrivateSceneToken(string sceneId)
        {
            return _scene.RpcTask<string, string>("sceneauthorization.gettoken", sceneId);
        }

        public async Task<AuthenticationResult> Authenticate(Client client)
        {
            _scene = await client.GetPublicScene("authenticator", true);
            var login = UserGenerator.Instance.GetLoginPassword();
            await _scene.Connect();
            Result = await Login(login.Item1, login.Item2);
            if (!Result.Success)
            {
                Console.WriteLine($"Authentication failed for {login.Item1}: "+Result.ErrorMsg);

                Console.WriteLine($"Creating account {login.Item1} , {login.Item2}");
                Result = await CreateAccount(login.Item1, login.Item2);
                if(Result.Success)
                {
                    Result = await Login(login.Item1, login.Item2);

                    if(!Result.Success)
                    {
                        Console.WriteLine($"Authentication failed for {login.Item1}: " + Result.ErrorMsg);
                    }
                }
                else
                {
                    Console.WriteLine($"Failed creating account {login.Item1} , {Result.ErrorMsg}");
                }
            }
            //client.Disconnect();
            return Result;
        }
        public AuthenticationResult Result { get; private set; }
        private async Task<AuthenticationResult> CreateAccount(string login, string password)
        {
            return await _scene.Rpc<CreateAccountRequest, AuthenticationResult>("provider.loginpassword.createAccount", new CreateAccountRequest { Login = login, Password = password, Email = login+"@elves.net", UserData = (new JObject()).ToString() });
        }
        private async Task<AuthenticationResult> Login(string login, string password)
        {
            var ctx = new Dictionary<string, string> {
                { "provider", "loginpassword" },
                { "login",login },
                {"password",password }
            };
            return await _scene.Rpc<Dictionary<string, string>, AuthenticationResult>("login", ctx);
        }
      
    }


    public class AuthenticationResult
    {
        public bool Success { get; set; }

        public string Token { get; set; }

        public string ErrorMsg { get; set; }
    }

    public class CreateAccountRequest
    {
        [MessagePackMember(0)]
        public string Login { get; set; }

        [MessagePackMember(1)]
        public string Password { get; set; }

        [MessagePackMember(2)]
        public string Email { get; set; }

        //Json userdata
        [MessagePackMember(3)]
        public string UserData { get; set; }
    }
}
