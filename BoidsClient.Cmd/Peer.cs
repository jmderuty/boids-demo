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

namespace BoidsClient.Cmd
{
    public class Peer
    {
        private Simulation _simulation;
        private ushort id;
        private bool _isRunning;
        private TimeSpan interval = TimeSpan.FromMilliseconds(200);
        private static int boidFrameSize = 2 + 3 * 4 + 4 + 4;

        private readonly string _name;

        private string _accountId;
        private string _app;
        private string _sceneId;

        public Peer(string name, string accountId, string appName, string sceneId)
        {
            _name = name;
            _app = appName;
            _accountId = accountId;
            _sceneId = sceneId;
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
            //config.Logger = new Logger();
            var client = new Stormancer.Client(config);

            var scene = await client.GetPublicScene(sceneName, new PlayersInfos { isObserver = false });

            scene.AddRoute("position.update", OnPositionUpdate);
            scene.AddRoute("ship.remove", OnShipRemoved);
            scene.AddRoute("ship.add", OnShipAdded);
            scene.AddRoute("ship.me", OnGetMyShipInfos);

            await scene.Connect();

            _scene = scene;
            _offset = (uint)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) % uint.MaxValue;
            _clock.Start();
            IsRunning = true;
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
                using (var writer = new BinaryWriter(new MemoryStream(_buffer)))
                {
                    writer.Write(id);
                    writer.Write(_simulation.Boid.X);
                    writer.Write(_simulation.Boid.Y);
                    writer.Write(_simulation.Boid.Rot);
                    writer.Write(_offset + (uint)_clock.ElapsedMilliseconds);
                    writer.Write(_packetIndex);
                }
                var tWrite = watch.ElapsedMilliseconds;
                Metrics.Instance.GetRepository("write").AddSample(id, tWrite);
                _scene.SendPacket("position.update", s => s.Write(_buffer, 0, boidFrameSize), PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);

                var tSend = watch.ElapsedMilliseconds;
                Metrics.Instance.GetRepository("send").AddSample(id, tSend - tWrite);

                _simulation.Step();
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
            _simulation = new Simulation(dto.x, dto.y, dto.rot);
        }

        private void OnShipAdded(Packet<IScenePeer> obj)
        {
            if (_simulation != null)
            {
                var shipInfos = obj.ReadObject<ShipCreatedDto>();
                if (shipInfos.id != this.id)
                {
                    var ship = new Ship { Id = shipInfos.id, X = shipInfos.x, Y = shipInfos.y, Rot = shipInfos.rot };
                    Console.WriteLine("[" + _name + "] Ship {0} added ", shipInfos.id);
                    _simulation.Environment.AddShip(ship);
                }
            }
        }

        private void OnShipRemoved(Packet<IScenePeer> obj)
        {
            if (_simulation != null)
            {
                var id = obj.ReadObject<ushort>();
                Console.WriteLine("[" + _name + "] Ship {0} removed ", id);
                _simulation.Environment.RemoveShip(id);
            }
        }

        private void OnPositionUpdate(Packet<IScenePeer> obj)
        {
            if (_simulation != null)
            {
                using (var reader = new BinaryReader(obj.Stream))
                {
                    reader.ReadByte();
                    var serverTime = reader.ReadUInt32();
                    while (reader.BaseStream.Length - reader.BaseStream.Position >= boidFrameSize)
                    {
                        var id = reader.ReadUInt16();
                        var x = reader.ReadSingle();
                        var y = reader.ReadSingle();
                        var rot = reader.ReadSingle();
                        var time = reader.ReadUInt32();
                        var packetIndex = reader.ReadUInt32();
                        if (id != this.id)
                        {
                            _simulation.Environment.UpdateShipLocation(id, x, y, rot);
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            _scene.Disconnect();
            _isRunning = false;
            IsRunning = false;
        }

        public bool IsRunning { get; set; }
    }
}
