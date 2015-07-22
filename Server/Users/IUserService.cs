using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stormancer;

namespace Server.Users
{
    public interface IUserService
    {
        Task<User> GetUser(IScenePeerClient peer);

        Task UpdateUserData<T>(IScenePeerClient peer,T data);

        Task AddAuthentication(User user,string provider, object authData); 

        bool IsAuthenticated(IScenePeerClient peer);

        Task<User> GetUserByClaim(string provider, string claimPath, string login);

        Task<User> CreateUser(string v, JObject userData);


    }
}
