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
        private Simulation _simulation;
        private ushort id;
       
        private bool _isRunning;
        private TimeSpan interval = TimeSpan.FromMilliseconds(200);
        private static int boidFrameSize = 22;

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
            _simulation = new Simulation();
            _simulation.Boid.CanAttack = canAttack;
        }

        public Task Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Console.WriteLine("Peer started");
                return Initialize();
            }
            return Task.FromResult(true);
        }
        public Action Stopped { get; set; }
        private class Logger : ILogger
        {
            public void Log(LogLevel level, string category, string message, object data)
            {
                Console.WriteLine(message);
            }
        }

        public async Task Initialize()
        {
            var accountId = _accountId;
            var applicationName = _app;
            var sceneName = _sceneId;
            var config = Stormancer.ClientConfiguration.ForAccount(accountId, applicationName);
            config.AsynchrounousDispatch = false;
            config.ServerEndpoint = _apiEndpoint;
            //config.Logger = new Logger();
            _client = new Stormancer.Client(config);
            _simulation.Boid.Clock = () => _client.Clock;
            _simulation.Boid.Fire = async (target, w) => (await _scene.RpcTask<UserSkillRequest, UseSkillResponse>("skill", new UserSkillRequest { skillId = w.id, target = target.Id }));
            var scene = await _client.GetPublicScene(sceneName, new PlayersInfos { isObserver = false });

            scene.AddRoute("position.update", OnPositionUpdate);
            scene.AddRoute("ship.remove", OnShipRemoved);
            scene.AddRoute("ship.add", OnShipAdded);
            scene.AddRoute("ship.me", OnGetMyShipInfos);
            scene.AddRoute("ship.statusChanged", OnShipStatusChanged);
            scene.AddRoute("ship.usedSkill", OnShipUsedSkill);
            await scene.Connect();

            _scene = scene;
            _offset = (uint)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) % uint.MaxValue;
            _clock.Start();
            IsRunning = true;
        }

        private void OnShipUsedSkill(Packet<IScenePeer> obj)
        {
            //We don't care
        }

        private void OnShipStatusChanged(Packet<IScenePeer> obj)
        {
            var statusChangedArgs = obj.ReadObject<StatusChangedMsg>();
            if (statusChangedArgs.shipId != this.id)
            {
                _simulation.Environment.VisibleShips[statusChangedArgs.shipId].Status = (ShipStatus)statusChangedArgs.status;
            }
            else
            {
                _simulation.Boid.Status = statusChangedArgs.status;
               
                Console.WriteLine("Ship {0} changed status to {1}", id, _simulation.Boid.Status);
            }
            //Update ship status for AI.
        }


        private Stormancer.Scene _scene;

        private uint _offset;
        private Stopwatch _clock = new Stopwatch();
        private byte[] _buffer = new byte[boidFrameSize];
        private uint _packetIndex = 0u;
        public void Run()
        {
            var watch = new Stopwatch();

            watch.Start();
            var current = DateTime.UtcNow;

            if (_simulation != null)
            {
                long tSend = watch.ElapsedMilliseconds;
                if (_simulation.Boid.Status == ShipStatus.InGame)
                {
                    using (var writer = new BinaryWriter(new MemoryStream(_buffer)))
                    {
                        writer.Write(id);
                        writer.Write(_simulation.Boid.X);
                        writer.Write(_simulation.Boid.Y);
                        writer.Write(_simulation.Boid.Rot);
                        writer.Write(_client.Clock);

                    }
                    var tWrite = watch.ElapsedMilliseconds;
                    Metrics.Instance.GetRepository("write").AddSample(id, tWrite);
                    _scene.SendPacket("position.update", s => s.Write(_buffer, 0, boidFrameSize), PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);

                    tSend = watch.ElapsedMilliseconds;
                    Metrics.Instance.GetRepository("send").AddSample(id, tSend - tWrite);
                }
                lock (_simulation.Environment)
                {
                    _simulation.Step();
                }
                var tSim = watch.ElapsedMilliseconds;
                Metrics.Instance.GetRepository("sim").AddSample(id, tSim - tSend);
                _packetIndex++;
            }

            watch.Stop();

        }

        private void OnGetMyShipInfos(Packet<IScenePeer> obj)
        {
            var dto = obj.ReadObject<ShipCreatedDto>();
            Console.WriteLine("[" + _name + "] Ship infos received : {0}", dto.id);
            id = dto.id;
            _simulation.Boid.Id = id;
            _simulation.Boid.X = dto.x;
            _simulation.Boid.Y = dto.y;
            _simulation.Boid.Rot = dto.rot;//= new Simulation(dto.x, dto.y, dto.rot);
            _simulation.Boid.Weapons = dto.weapons.ToList();
        }

        private void OnShipAdded(Packet<IScenePeer> obj)
        {
            
            if (_simulation != null)
            {
                while (obj.Stream.Position != obj.Stream.Length)
                {
                    var shipInfos = obj.ReadObject<ShipCreatedDto>();
                    if (shipInfos.id != this.id)
                    {
                        var ship = new Ship { Id = shipInfos.id, Team = shipInfos.team, X = shipInfos.x, Y = shipInfos.y, Rot = shipInfos.rot, Weapons = shipInfos.weapons };
                        Console.WriteLine("[" + _name + "] Ship {0} added ", shipInfos.id);
                        lock (_simulation.Environment)
                        {
                            _simulation.Environment.AddShip(ship);
                        }
                    }
                }
            }
        }

        private void OnShipRemoved(Packet<IScenePeer> obj)
        {
            if (_simulation != null)
            {
                var id = obj.ReadObject<ushort>();
                Console.WriteLine("[" + _name + "] Ship {0} removed ", id);
                lock (_simulation.Environment)
                {
                    _simulation.Environment.RemoveShip(id);
                }
            }
        }

        private void OnPositionUpdate(Packet<IScenePeer> obj)
        {
            if (_simulation != null)
            {
                using (var reader = new BinaryReader(obj.Stream))
                {
                   
                    while (reader.BaseStream.Length - reader.BaseStream.Position >= boidFrameSize)
                    {
                        var id = reader.ReadUInt16();
                        var x = reader.ReadSingle();
                        var y = reader.ReadSingle();
                        var rot = reader.ReadSingle();
                        var time = reader.ReadInt64();

                        if (id != this.id)
                        {
                            _simulation.Environment.UpdateShipLocation(id, x, y, rot);
                        }
                        else if (_simulation.Boid.Status != ShipStatus.InGame)
                        {
                            _simulation.Boid.X = x;
                            _simulation.Boid.Y = y;
                            _simulation.Boid.Rot = rot;
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            if (IsRunning && _scene != null)
            {
                _scene.Disconnect();
            }
            _isRunning = false;
            IsRunning = false;
        }

        public bool IsRunning { get; set; }
    }
}
