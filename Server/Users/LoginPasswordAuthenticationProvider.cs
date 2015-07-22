using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using Stormancer;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography;
using System.Globalization;

namespace Server.Users
{
    class LoginPasswordAuthenticationProvider : IAuthenticationProvider
    {
        private const string PROVIDER_NAME = "loginpassword";
        private const int SaltValueSize = 32;
        public void AddMetadata(Dictionary<string, string> result)
        {
            result.Add("provider.loginpassword", "enabled");
        }

        public void AdjustScene(ISceneHost scene)
        {
            scene.AddProcedure("provider.loginpassword.createAccount", async p =>
            {

                var userService = scene.GetComponent<IUserService>();
                var rq = p.ReadObject<CreateAccountRequest>();

                ValidateLoginPassword(rq.Login, rq.Password);

                var user = await userService.GetUserByClaim(PROVIDER_NAME, "login", rq.Login);

                if (user != null)
                {
                    throw new ClientException("An user with this login already exist.");
                }

                user = await userService.GetUser(p.RemotePeer);
                if (user == null)
                {
                    try
                    {

                        await userService.CreateUser(PROVIDER_NAME + "-" + rq.Login, rq.UserData);
                    }
                    catch (Exception ex)
                    {
                        throw new ClientException("Couldn't create account : " + ex.Message);
                    }
                }

                var salt = GenerateSaltValue();

                try
                {
                    await userService.AddAuthentication(user, PROVIDER_NAME, new
                    {
                        login = rq.Login,
                        email = rq.Email,
                        salt = salt,
                        password = HashPassword(rq.Password, salt),
                        validated = false,
                    });
                }
                catch (Exception ex)
                {
                    throw new ClientException("Couldn't link account : " + ex.Message);
                }




            });
        }

        private void ValidateLoginPassword(string login, string password)
        {
            if (string.IsNullOrEmpty(login))
            {
                throw new ClientException("User id must be non null or empty.");

            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"^[\w|_@-]+$"))
            {
                throw new ClientException("User id must contain alphanumeric characters, _ , @ or -.");
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new ClientException("Password must be non null or empty.");
            }
            if (password.Length < 6)
            {
                throw new ClientException("Password must be more than 6 characters long.");
            }
            var complexityScore = 0;

            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
            {
                complexityScore += 1;
            }

            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
            {
                complexityScore += 1;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(password, "[a-z]"))
            {
                complexityScore += 1;
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(password, @"\W|_"))
            {
                complexityScore += 1;
            }

            if (complexityScore < 3)
            {
                throw new ClientException("Password must contain at least 3 types of characters among lowercase, uppercase, numbers and  non word characters.");
            }
        }

        public async Task<bool> Authenticate(Dictionary<string, string> authenticationCtx, IUserService _userService)
        {
            if(authenticationCtx["provider"] != PROVIDER_NAME)
            {
               
                return false;
            }

            var login = authenticationCtx["login"];
            var password = authenticationCtx["password"];
            if(string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                throw new ClientException("Login and password must be non empty.");
            }

            var user = await _userService.GetUserByClaim(PROVIDER_NAME, "login", login);
            if(user == null)
            {
                throw new ClientException("No user found that matches the provided login/password.");
            }

            dynamic authData = user.Auth[PROVIDER_NAME];

            string salt = authData.salt;
            string hash = authData.password;

            var candidateHash = HashPassword(password, salt);
            if(hash != candidateHash)
            {
                throw new ClientException("No user found that matches the provider login/password.");
            }
            return true;
        }


        private string GenerateSaltValue()
        {

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789&é'(-è_çà)=$*ù%*µ!:;.?,";


            Random random = new Random();

            if (random != null)
            {
                // Create an array of random values.

                char[] saltValue = new char[SaltValueSize];

                for (int i = 0; i < saltValue.Length; i++)
                {
                    saltValue[i] = chars[random.Next(chars.Length)];
                }
                return new string(saltValue);

            }

            return null;
        }
        private string HashPassword(string password, string salt)
        {
            return Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
        }


    }

    public class CreateAccountRequest
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }

        public JObject UserData { get; set; }
    }
}
