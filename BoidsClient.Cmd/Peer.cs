﻿using Server;
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
        private int boidFrameSize = 2 + 3 * 4 + 4 + 4;

        private readonly string _name;

        private string _accountId;
        private string _app;
        private string _scene;

        public Peer(string name,string accountId, string appName, string scene)
        {
            _name = name;
            _app = appName;
            _accountId = accountId;
            _scene = scene;
        }

        public void Start()
        {
            if (!_isRunning)
            {
                _isRunning = true;
                Console.WriteLine("Peer started");
                RunImpl().ContinueWith(t=> {

                    var stopped = Stopped;
                    if(stopped !=null)
                    {
                        stopped();
                    }
                });
            }
        }
        public Action Stopped { get; set; }
        private class Logger : ILogger
        {
            public void Log(LogLevel level, string category, string message, object data)
            {
                Console.WriteLine(message);
            }
        }

        private async Task RunImpl()
        {

            var accountId =_accountId;
            var applicationName = _app;
            var sceneName = _scene;

            var packetIndex = 0u;


            var config = Stormancer.ClientConfiguration.ForAccount(accountId, applicationName);
            //config.Logger = new Logger();
            var client = new Stormancer.Client(config);
            Console.WriteLine("start");

            var scene = await client.GetPublicScene(sceneName, new PlayersInfos { isObserver = false });
            Console.WriteLine("retrieved scene");
            scene.AddRoute("position.update", OnPositionUpdate);
            scene.AddRoute("ship.remove", OnShipRemoved);
            scene.AddRoute("ship.add", OnShipAdded);
            scene.AddRoute("ship.me", OnGetMyShipInfos);

            await scene.Connect();
            Console.WriteLine("connected");
            var buffer = new byte[boidFrameSize];
            uint serverClock = (uint)(DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond) % uint.MaxValue;
            var clock = new Stopwatch();
            clock.Start();
            var watch = new Stopwatch();
            while (_isRunning)
            {
                watch.Restart();
                var current = DateTime.UtcNow;

                if (_simulation != null)
                {
                    using (var writer = new BinaryWriter(new MemoryStream(buffer)))
                    {
                        writer.Write(id);
                        writer.Write(_simulation.Boid.X);
                        writer.Write(_simulation.Boid.Y);
                        writer.Write(_simulation.Boid.Rot);
                        writer.Write(serverClock + (uint)clock.ElapsedMilliseconds);
                        writer.Write(packetIndex);
                    }
                    scene.SendPacket("position.update", s => s.Write(buffer, 0, boidFrameSize), PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);
                    _simulation.Step();
                }
                packetIndex++;
                watch.Stop();
                var delay = 200 - watch.ElapsedMilliseconds;
                if (delay > 0)
                {
                    var watch2 = new Stopwatch();
                    watch2.Start();
                    await Task.Delay((int)delay);
                    watch2.Stop();
                    Metrics.Instance.GetRepository("found_intervals").AddSample(id, watch2.ElapsedMilliseconds);
                }
                
                Metrics.Instance.GetRepository("expected_intervals").AddSample(id, delay);
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
            _isRunning = false;
        }
    }
}
