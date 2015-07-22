using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Stormancer;

namespace Server.Users
{
    class UserService : IUserService
    {
        public UserService(UserManagementConfig config)
        {

        }

        public Task AddAuthentication(User user, string provider, object authData)
        {
            throw new NotImplementedException();
        }

        public Task<User> CreateUser(string v, JObject userData)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(IScenePeerClient peer)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUserByClaim(string provider, string claimPath, string login)
        {
            throw new NotImplementedException();
        }

        public bool IsAuthenticated(IScenePeerClient peer)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUserData<T>(IScenePeerClient peer, T data)
        {
            throw new NotImplementedException();
        }
    }
}
