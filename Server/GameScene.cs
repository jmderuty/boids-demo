using Stormancer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using System.Collections.Concurrent;
using Stormancer.Diagnostics;
using System.Diagnostics;

namespace Server
{
    static class GameSceneExtensions
    {
        public static void AddGameScene(this IAppBuilder builder)
        {
            builder.SceneTemplate("game", scene => new GameScene(scene));
        }
    }

    class GameScene
    {
        private const float X_MIN = -100;
        private const float X_MAX = 100;
        private const float Y_MIN = -100;
        private const float Y_MAX = 100;

        private readonly ISceneHost _scene;
        private ushort _currentId = 0;
        private ConcurrentDictionary<long, Player> _players = new ConcurrentDictionary<long, Player>();
        private ConcurrentDictionary<ushort, Ship> _ships = new ConcurrentDictionary<ushort, Ship>();

        private bool isRunning = false;

        private TimeSpan interval = TimeSpan.FromMilliseconds(200);

        Stopwatch stopWatch = new Stopwatch();

        public GameScene(ISceneHost scene)
        {
            _scene = scene;

            _scene.Connected.Add(OnConnected);
            _scene.Disconnected.Add(OnDisconnected);
            _scene.AddRoute("position.update", OnPositionUpdate);
            _scene.Starting.Add(OnStarting);
        }

        private Task OnStarting(dynamic arg)
        {
            StartUpdateLoop();
            return Task.FromResult(true);
        }

        private void StartUpdateLoop()
        {
            if (!isRunning)
            {
                isRunning = true;
                Task.Run(RunUpdate);
            }
        }

        private async Task RunUpdate()
        {
            var lastRun = DateTime.MinValue;
            _scene.GetComponent<ILogger>().Info("gameScene", "Starting update loop");
            var lastLog = DateTime.MinValue;
            stopWatch.Start();
            var metrics = new ConcurrentDictionary<int, uint>();
            while (isRunning)
            {
                try
                {
                    var current = DateTime.UtcNow;

                    if (current > lastRun + interval && _scene.RemotePeers.Any())
                    {
                        if (_ships.Any(s => s.Value.PositionUpdatedOn > lastRun))
                        {
                            _scene.Broadcast("position.update", s =>
                            {
                                var nb = 0;
                                foreach (var ship in _ships.Values.ToArray())
                                {
                                    if (ship.PositionUpdatedOn > lastRun)
                                    {
                                        s.Write(ship.LastPositionRaw, 0, ship.LastPositionRaw.Length);
                                        nb++;
                                    }
                                }
                                metrics.AddOrUpdate(nb, 1, (i, old) => old + 1);
                            }, PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);
                        }

                        lastRun = current;
                        if (current > lastLog + TimeSpan.FromMinutes(1))
                        {
                            lastLog = current;

                            _scene.GetComponent<ILogger>().Log(LogLevel.Info, "gameloop", "running", new
                            {
                                sends = metrics.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                                received = ComputeMetrics()
                            });
                            metrics.Clear();
                        }

                        await Task.Delay(current + interval - DateTime.UtcNow);
                    }
                }
                catch (Exception ex)
                {
                    _scene.GetComponent<ILogger>().Error("update.loop", "{0}", ex.Message);
                    throw;
                }
            }

            stopWatch.Stop();
        }
        public class ReceivedDataMetrics
        {
            public double Avg;
            public int NbSamples;
            public int[] Percentiles = new int[11];
        }

        public ReceivedDataMetrics ComputeMetrics()
        {


            var intervals = new List<int>();
            foreach (var boid in _boidsTimes)
            {
                var values = boid.Value.ToArray();
                for (int i = 1; i < values.Length; i++)
                {
                    intervals.Add((int)(values[i] - values[i - 1]));
                }
            }


            var result = new ReceivedDataMetrics();
            if (intervals.Any())
            {
                intervals.Sort();

                result.Avg = intervals.Cast<int>().Average();
                result.NbSamples = intervals.Count;
                for (int i = 0; i < 11; i++)
                {
                    result.Percentiles[i] = intervals[(i * result.NbSamples) / 10];
                }
                //result.Percentiles[10] = intervals[result.NbSamples];
            }
            return result;
        }

        private ConcurrentDictionary<ushort, List<uint>> _boidsTimes = new ConcurrentDictionary<ushort, List<uint>>();
        private void OnPositionUpdate(Packet<IScenePeerClient> packet)
        {
            unchecked
            {
                var timestamp = (uint)stopWatch.ElapsedMilliseconds;


                var bytes = new byte[18];
                packet.Stream.Read(bytes, 0, 14);

                var shipId = BitConverter.ToUInt16(bytes, 0);
                Ship ship;
                if (_ships.TryGetValue(shipId, out ship))
                {
                    ship.PositionUpdatedOn = DateTime.UtcNow;
                    ship.LastPositionRaw = bytes;
                }
                _boidsTimes.AddOrUpdate(shipId, _ => new List<uint> { timestamp }, (_, l) => { l.Add(timestamp); return l; });
                byte[] time = BitConverter.GetBytes(timestamp);
                for (var i = 0; i < sizeof(uint); i++)
                {
                    bytes[14 + i] = time[i];
                }
            }
        }

        private async Task OnDisconnected(DisconnectedArgs arg)
        {
            Player player;
            if (_players.TryRemove(arg.Peer.Id, out player))
            {
                Ship ship;
                if (_ships.TryRemove(player.ShipId, out ship))
                {
                    List<uint> _;
                    _boidsTimes.TryRemove(player.ShipId, out _);
                    _scene.GetComponent<ILogger>().Info("gameScene", "removed ship");
                    _scene.Broadcast("ship.remove", s => arg.Peer.Serializer().Serialize(ship.id, s), PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE);
                }
            }
        }

        private async Task OnConnected(IScenePeerClient client)
        {
            var pInfos = PlayerInfos.FromPeer(client);

            var player = new Player(pInfos, client.Id);

            _players.AddOrUpdate(client.Id, player, (id, old) => player);
            if (!player.IsObserver)
            {
                var ship = CreateShip(player);

                _ships.AddOrUpdate(ship.id, ship, (id, old) => ship);

                var dto = new ShipCreatedDto { id = ship.id, x = ship.x, y = ship.y, rot = ship.rot };
                client.Send("ship.me", s => client.Serializer().Serialize(dto, s), PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE);

                var peersBySerializer = _scene.RemotePeers.ToLookup(peer => peer.Serializer().Name);

                foreach (var group in peersBySerializer)
                {
                    _scene.Send(new MatchArrayFilter(group), "ship.add", s =>
                        {
                            group.First().Serializer().Serialize(dto, s);
                        }, PacketPriority.MEDIUM_PRIORITY, PacketReliability.RELIABLE);
                }
            }
            _scene.GetComponent<ILogger>().Info("gameScene", "Added ship");
            StartUpdateLoop();
        }

        private Random _rand = new Random();

        private Ship CreateShip(Player player)
        {
            ushort id = 0;
            lock (this)
            {
                id = _currentId++;
            }
            player.ShipId = id;
            var ship = new Ship
            {
                id = id,
                player = player,
                rot = (float)(_rand.NextDouble() * 2 * Math.PI),
                x = X_MIN + (float)(_rand.NextDouble() * (X_MAX - X_MIN)),
                y = Y_MIN + (float)(_rand.NextDouble() * (Y_MAX - Y_MIN))
            };
            return ship;
        }
    }
}
