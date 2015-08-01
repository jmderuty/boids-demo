using System;
using System.Collections.Concurrent;
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
        private Database.ESClientFactory _clientFactory;
        private string _indexName =Constants.INDEX;

        public UserService(UserManagementConfig config, Database.ESClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

       
        private async Task<Nest.IElasticClient> Client()
        {
            return await _clientFactory.CreateClient(_indexName);
        }
        public async Task<User> AddAuthentication(User user, string provider, JObject authData)
        {
            var c = await Client();
            var r = await c.GetAsync<User>(gd => gd.Id(user.Id));
            r.Source.Auth["provider"] = authData;
            
            await (await Client()).IndexAsync(r.Source);
            return r.Source;
        }

        public async Task<User> CreateUser(string id, JObject userData)
        {
            var user = new User() { Id = id, UserData = userData };

            await (await Client()).IndexAsync(user);
            return user;
        }

        public void SetUid(IScenePeerClient peer, string id)
        {
            peer.Metadata["uid"] = id;
        }

        public async Task<User> GetUser(IScenePeerClient peer)
        {
            string id;
            if(!peer.Metadata.TryGetValue(peer.Metadata["uid"],out id))
            {
                return null;
            }
            
            var c = await Client();
            var r = await c.GetAsync<User>(gd => gd.Id(id));

            return r.Source;
        }

        public async Task<User> GetUserByClaim(string provider, string claimPath, string login)
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
