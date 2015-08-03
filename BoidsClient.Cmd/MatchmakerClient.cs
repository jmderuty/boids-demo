using System;
using System.Threading.Tasks;
using Server.Matchmaking;
using Stormancer;
using System.Reactive.Linq;

namespace BoidsClient.Cmd
{
    internal class MatchmakerClient
    {
        private readonly Client _client;
        private Scene _scene;
        public MatchmakerClient(Client client)
        {
            _client = client;
        }

        internal async Task Connect(string token)
        {
            _scene = await _client.GetScene(token);
            await _scene.Connect();

            
        }

        public async Task<FindMatchResult> FindMatch()
        {
            return await _scene.Rpc<bool, FindMatchResult>("match.find", true);
        }
    }
}