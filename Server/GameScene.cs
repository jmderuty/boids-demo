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
using System.Threading;
using Stormancer.Plugins;
using System.IO;

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

        Stopwatch clock = new Stopwatch();

        public GameScene(ISceneHost scene)
        {
            _scene = scene;

            _scene.Connected.Add(OnConnected);
            _scene.Disconnected.Add(OnDisconnected);
            _scene.AddRoute("position.update", OnPositionUpdate);
            _scene.AddProcedure("clock", ClockRequest);
            _scene.Starting.Add(OnStarting);
        }

        private Task ClockRequest(RequestContext<IScenePeerClient> arg)
        {
            arg.SendValue(s =>
            {
                using (var w = new BinaryWriter(s, Encoding.UTF8, true))
                {
                    w.Write((uint)clock.ElapsedMilliseconds);
                }
                arg.InputStream.CopyTo(s);
            }, PacketPriority.IMMEDIATE_PRIORITY);
            return Task.FromResult(true);
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
                Task.Run(()=>RunUpdate());
            }
        }

        private async Task RunUpdate()
        {
            var lastRun = DateTime.MinValue;
            _scene.GetComponent<ILogger>().Info("gameScene", "Starting update loop");
            var lastLog = DateTime.MinValue;
            clock.Start();
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
                                var binWriter = new BinaryWriter(s);
                                binWriter.Write((byte)0xc0);
                                binWriter.Write((uint)clock.ElapsedMilliseconds);
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
                        else
                        {
                            metrics.AddOrUpdate(0, 1, (i, old) => old + 1);
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
                        var execution = DateTime.UtcNow - current;
                        if (execution > this._longestExecution)
                        {
                            this._longestExecution = execution;
                        }

                        var delay = interval - execution;
                        if (delay > TimeSpan.Zero)
                        {
                            await Task.Delay(delay);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _scene.GetComponent<ILogger>().Error("update.loop", "{0}", ex.Message);
                    throw;
                }
            }

            clock.Stop();
        }

        public class ReceivedDataMetrics
        {
            public double Avg;
            public int NbSamples;
            public int[] Percentiles = new int[11];
            public int Percentile99;
            public int LostPackets;
            public double LongestExecution;
        }

        public ReceivedDataMetrics ComputeMetrics()
        {
            var intervals = new List<int>();
            foreach (var boid in _boidsTimes)
            {
                var values = boid.Value.ToArray();
                boid.Value.Clear();
                for (int i = 0; i < values.Length; i++)
                {
                    intervals.Add((int)values[i]);
                    //intervals.Add((int)(values[i] - values[i - 1]));
                }
            }

            var result = new ReceivedDataMetrics();
            if (intervals.Any())
            {
                intervals.Sort();

                result.Avg = intervals.Average();
                result.NbSamples = intervals.Count;
                for (int i = 0; i < 11; i++)
                {
                    result.Percentiles[i] = intervals[(i * (result.NbSamples - 1)) / 10];
                }
                result.Percentile99 = intervals[99 * (result.NbSamples - 1) / 100];
                result.LostPackets = this._lostPackets;
                this._lostPackets = 0;
                result.LongestExecution = this._longestExecution.TotalMilliseconds;
                this._longestExecution = TimeSpan.Zero;
            }
            return result;
        }

        private ConcurrentDictionary<ushort, List<long>> _boidsTimes = new ConcurrentDictionary<ushort, List<long>>();
        private ConcurrentDictionary<ushort, uint> _boidsLastIndex = new ConcurrentDictionary<ushort, uint>();
        private const int positionUpdateLength = 2 + 3*4 + 4 + 4;
        private int _lostPackets = 0;
        private TimeSpan _longestExecution = TimeSpan.Zero;
        private void OnPositionUpdate(Packet<IScenePeerClient> packet)
        {
            unchecked
            {
                var time = clock.ElapsedMilliseconds;
                var bytes = new byte[positionUpdateLength];
                packet.Stream.Read(bytes, 0, positionUpdateLength);

                var shipId = BitConverter.ToUInt16(bytes, 0);
                Ship ship;
                if (_ships.TryGetValue(shipId, out ship))
                {
                    ship.PositionUpdatedOn = DateTime.UtcNow;
                    ship.LastPositionRaw = bytes;
                }
                var boidTime = BitConverter.ToUInt32(bytes, 2 + 3*4);
                //var latency = (DateTime.UtcNow.Ticks - boidNow) / 10000;

                var packetIndex = BitConverter.ToUInt32(bytes, 2 + 3*4 + 4);
                this._boidsLastIndex.AddOrUpdate(shipId, packetIndex, (_, previousIndex) =>
                {
                    if (previousIndex < (packetIndex - 1))
                    {
                        Interlocked.Add(ref this._lostPackets, (int)(packetIndex - previousIndex - 1));
                    }
                    return packetIndex;
                });
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
                    List<long> _;
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
