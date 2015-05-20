using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stormancer.Core;
using Stormancer;
using Server;
using System.IO;
using Stormancer.Diagnostics;

namespace BoidsClient.Cmd
{
    public class Peer
    {
        private Simulation _simulation;
        private ushort id;
        private bool _isRunning;

        private readonly string _name;
        public Peer(string name)
        {
            _name = name;
        }
        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Console.WriteLine("Peer started");
                RunImpl();
            }
        }

        private class Logger : ILogger
        {
            public void Log(LogLevel level, string category, string message, object data)
            {
                Console.WriteLine(message);
            }
        }
        private async Task RunImpl()
        {
            var config = Stormancer.ClientConfiguration.ForAccount("d81fc876-6094-3d92-a3d0-86d42d866b96", "boids-demo");
            //config.Logger = new Logger();
            var client = new Stormancer.Client(config);
            Console.WriteLine("start");

            var scene = await client.GetPublicScene("main-session", new PlayersInfos { isObserver = false });
            Console.WriteLine("retrieved scene");
            scene.AddRoute("position.update", OnPositionUpdate);
            scene.AddRoute("ship.remove", OnShipRemoved);
            scene.AddRoute("ship.add", OnShipAdded);
            scene.AddRoute("ship.me", OnGetMyShipInfos);

            await scene.Connect();
            Console.WriteLine("connected");
            var buffer = new byte[14];
            while (_isRunning)
            {
                if (_simulation != null)
                {
                    using (var writer = new BinaryWriter(new MemoryStream(buffer)))
                    {
                        writer.Write(id);
                        writer.Write(_simulation.Boid.X);
                        writer.Write(_simulation.Boid.Y);
                        writer.Write(_simulation.Boid.Rot);
                    }
                    scene.SendPacket("position.update", s => s.Write(buffer, 0, 14), PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);
                    _simulation.Step();
                }
                await Task.Delay(200);
            }

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
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var id = reader.ReadUInt16();
                        var x = reader.ReadSingle();
                        var y = reader.ReadSingle();
                        var rot = reader.ReadSingle();
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
            _isRunning = false;
        }
    }
}
