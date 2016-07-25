using Server;
using Stormancer;
using Stormancer.Core;
using Stormancer.Diagnostics;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Linq;

namespace BoidsClient.Cmd
{
    public class Peer
    {

        private ushort id;

        private bool _isRunning;
        private TimeSpan interval = TimeSpan.FromMilliseconds(200);
        private static int boidFrameSize = 22;
        private bool _isReady = false;

        private readonly string _name;

        private string _accountId;
        private string _app;
        private string _sceneId;
        private string _apiEndpoint;
        private Client _client;
        public Peer(string name, string apiEndpoint, string accountId, string appName, string sceneId, bool canAttack)
        {
            _name = name;
            _app = appName;
            _accountId = accountId;
            _sceneId = sceneId;
            _apiEndpoint = apiEndpoint;
            var config = Stormancer.ClientConfiguration.ForAccount(accountId, appName);
            config.AsynchrounousDispatch = false;
            config.ServerEndpoint = apiEndpoint;
            _client = new Client(config);

        }

        public async Task Start()
        {
            IsRunning = true;
            var authenticator = new AuthenticatorClient();
            var result =  await authenticator.Authenticate(_client);
            
            if (!result.Success)
            {
                Console.WriteLine("Authentication failed : " + authenticator.Result.ErrorMsg);
                return;
            }
            var token = await authenticator.GetPrivateSceneToken("main");
            var game = new GameSessionClient(_name, token);
            await game.Start(_client);
            _currentHandler = game;
            await game.CompletedAsync();
            game.Stop();
            //var matchmaker = new MatchmakerClient(_client);
            //await matchmaker.Connect(result.Token);
            //while (IsRunning)
            //{
            //    var match = await matchmaker.FindMatch();

            //    var game = new GameSessionClient(_name, match.Token);
            //    await game.Start(_client);
            //    _currentHandler = game;

            //    await game.CompletedAsync();

            //    game.Stop();

            //}




        }
        public Action Stopped { get; set; }
        public bool IsRunning
        {
            get; private set;
        }

        private class Logger : ILogger
        {
            public void Log(LogLevel level, string category, string message, Exception ex)
            {
                Log(level, category, message, (object)ex);
            }

            public void Log(LogLevel level, string category, string message, object data)
            {
                Console.WriteLine(message);
            }
        }

        public void Run()
        {
            if (_currentHandler != null)
            {
                _currentHandler.Run();
            }
        }





        private IHandler _currentHandler;
        public void Stop()
        {

            if (_currentHandler != null)
            {
                _currentHandler.Stop();
            }
            IsRunning = false;
        }

    }
}
