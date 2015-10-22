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
using System.Security.Authentication;

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
            config.AddPlugin(new Stormancer.Authentication.AuthenticationPlugin());
            config.AsynchrounousDispatch = false;
            config.ServerEndpoint = apiEndpoint;
            _client = new Client(config);

        }

        public async Task Start()
        {
            IsRunning = true;


            var login = UserGenerator.Instance.GetLoginPassword();

            Scene matchMakerScene;
            try
            {
                try
                {
                    matchMakerScene = await _client.Authenticator().Login(login.Item1, login.Item2);
                }
                catch (InvalidCredentialException)
                {
                    await _client.Authenticator().CreateLoginPasswordAccount(login.Item1, login.Item2, login.Item1 + "@elves.net", new object());

                    // we wait for the account to become active
                    await Task.Delay(TimeSpan.FromSeconds(10));

                    matchMakerScene = await _client.Authenticator().Login(login.Item1, login.Item2);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Authentication failed : " + ex.Message);
                return;
            }

            var matchmaker = new MatchmakerClient(matchMakerScene);
            await matchmaker.Connect();
            while (IsRunning)
            {
                var match = await matchmaker.FindMatch();

                var game = new GameSessionClient(_name, match.Token);
                await game.Start(_client);
                _currentHandler = game;

                await game.CompletedAsync();

                game.Stop();

            }
            



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
                Console.WriteLine(message + ex);
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
