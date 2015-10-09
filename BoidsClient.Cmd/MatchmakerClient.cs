using System;
using System.Threading.Tasks;
using Server.Matchmaking;
using Stormancer;
using System.Reactive.Linq;

namespace BoidsClient.Cmd
{
    internal class MatchmakerClient
    {
        private readonly Scene _scene;
        public MatchmakerClient(Scene matchmakerScene)
        {
            _scene = matchmakerScene;
        }

        internal async Task Connect()
        {
            await _scene.Connect();
        }

        public async Task<FindMatchResult> FindMatch()
        {
            return await _scene.Rpc<bool, FindMatchResult>("match.find", true);
        }
    }
}