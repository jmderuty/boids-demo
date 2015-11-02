using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Server;
using Stormancer;
using Stormancer.Core;

namespace BoidsClient.Cmd
{

    class GameSessionClient : IHandler
    {
        private const long POSITION_UPDATE_FRAME_LENGTH = 22;

        private Stormancer.Client _client;
        private readonly string _token;
        private Stormancer.Scene _scene;
        private Simulation _simulation;
        private ushort id;
        private string _name;
        private bool _isReady;

        public GameSessionClient(string peerName, string token)
        {
            _name = peerName;
            _token = token;

            _simulation = new Simulation();
            _simulation.Boid.CanAttack = true;
        }

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

                    var tWrite = watch.ElapsedMilliseconds;
                    Metrics.Instance.GetRepository("write").AddSample(id, tWrite);
                    _scene.SendPacket("position.update", s =>
                    {
                        using (var writer = new BinaryWriter(s, Encoding.UTF8, true))
                        {
                            writer.Write(id);
                            writer.Write(_simulation.Boid.X);
                            writer.Write(_simulation.Boid.Y);
                            writer.Write(_simulation.Boid.Rot);
                            writer.Write(_client.Clock);
                        }


                    }, PacketPriority.MEDIUM_PRIORITY, PacketReliability.UNRELIABLE_SEQUENCED);

                    tSend = watch.ElapsedMilliseconds;
                    Metrics.Instance.GetRepository("send").AddSample(id, tSend - tWrite);
                    lock (_simulation.Environment)
                    {
                        _simulation.Step();
                    }
                }

                var tSim = watch.ElapsedMilliseconds;
                Metrics.Instance.GetRepository("sim").AddSample(id, tSim - tSend);
            }

            watch.Stop();
        }

        internal Task CompletedAsync()
        {
            return _gameCompleteTcs.Task;
        }
        private TaskCompletionSource<bool> _gameCompleteTcs = new TaskCompletionSource<bool>();
        public Task Start(Stormancer.Client client)
        {
            if (!IsRunning)
            {
                _client = client;

                return Initialize();


            }
            return Task.FromResult(true);
        }

        public void Stop()
        {
            if (IsRunning && _scene != null)
            {
                _scene.Disconnect();
            }

            IsRunning = false;
        }


        public bool IsRunning { get; set; }


        private async Task Initialize()
        {

            //var accountId = _accountId;
            //var applicationName = _app;
            //var sceneName = _sceneId;
            //var config = Stormancer.ClientConfiguration.ForAccount(accountId, applicationName);
            //config.AsynchrounousDispatch = false;
            //config.ServerEndpoint = _apiEndpoint;
            //config.Logger = new Logger();
            //_client = new Stormancer.Client(config);
            _simulation.Boid.Clock = () => _client.Clock;
            _simulation.Boid.Fire = async (target, w) => (await _scene.RpcTask<UserSkillRequest, UseSkillResponse>("skill", new UserSkillRequest { skillId = w.id, target = target.Id }));
            var scene = await _client.GetScene(_token);

            scene.AddRoute("position.update", OnPositionUpdate);
            scene.AddRoute("ship.remove", OnShipRemoved);
            scene.AddRoute("ship.add", OnShipAdded);
            scene.AddRoute("ship.me", OnGetMyShipInfos);
            scene.AddRoute("ship.statusChanged", OnShipStatusChanged);
            scene.AddRoute("ship.usedSkill", OnShipUsedSkill);
            scene.AddRoute("ship.forcePositionUpdate", OnForcePositionUpdate);
            scene.AddRoute("game.statusChanged", OnGameStatusChanged);
            await scene.Connect();

            _scene = scene;

            IsRunning = true;
        }

        private void OnGameStatusChanged(Packet<IScenePeer> obj)
        {
            using (var reader = new BinaryReader(obj.Stream))
            {
                var status = (GameStatus)reader.ReadByte();
                if(status == GameStatus.Complete)
                {
                    _gameCompleteTcs.SetResult(true);
                }
            }
        }

        private void OnForcePositionUpdate(Packet<IScenePeer> obj)
        {
            using (var reader = new BinaryReader(obj.Stream))
            {
                var id = reader.ReadUInt16();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var rot = reader.ReadSingle();
                var time = reader.ReadInt64();

                if (id == this.id)
                {
                    _simulation.Boid.X = x;
                    _simulation.Boid.Y = y;
                    _simulation.Boid.Rot = rot;
                }
            }

        }

        private void OnPositionUpdate(Packet<IScenePeer> obj)
        {
            if (!_isReady)
            {
                return;
            }
            if (_simulation != null)
            {
                using (var reader = new BinaryReader(obj.Stream))
                {

                    while (reader.BaseStream.Length - reader.BaseStream.Position >= POSITION_UPDATE_FRAME_LENGTH)
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
                        }
                    }
                }
            }
        }

        private void OnShipUsedSkill(Packet<IScenePeer> obj)
        {
            //We don't care
        }

        private void OnShipStatusChanged(Packet<IScenePeer> obj)
        {
            if (!_isReady)
            {
                return;
            }
            var statusChangedArgs = obj.ReadObject<StatusChangedMsg>();
            if (statusChangedArgs.shipId != this.id)
            {
                if (_simulation.Environment.VisibleShips.ContainsKey(statusChangedArgs.shipId))
                {


                    _simulation.Environment.VisibleShips[statusChangedArgs.shipId].Status = (ShipStatus)statusChangedArgs.status;
                }
            }
            else
            {
                if (_simulation.Boid.Status != ShipStatus.InGame && statusChangedArgs.status == ShipStatus.InGame)
                {
                    _simulation.Reset();
                }
                _simulation.Boid.Status = statusChangedArgs.status;


            }

        }

        private void OnGetMyShipInfos(Packet<IScenePeer> obj)
        {
            var dtos = obj.ReadObject<ShipCreatedDto[]>();
            if (dtos.Length != 1)
            {
                throw new InvalidDataException("Invalid number of dtos");
            }

            var dto = dtos[0];
            Console.WriteLine("[" + _name + "] Ship infos received : {0}", dto.id);
            id = dto.id;
            _simulation.Boid.Id = id;
            _simulation.Boid.X = dto.x;
            _simulation.Boid.Y = dto.y;
            _simulation.Boid.Rot = dto.rot;//= new Simulation(dto.x, dto.y, dto.rot);
            _simulation.Boid.Weapons = dto.weapons.Select(w => new WeaponViewModel { Weapon = w }).ToList();
            _isReady = true;
        }

        private void OnShipAdded(Packet<IScenePeer> obj)
        {
            //Console.WriteLine("ship.add received");
            if (_simulation != null && _isReady)
            {
                var shipsToAdd = obj.ReadObject<ShipCreatedDto[]>();
                //while (obj.Stream.Position != obj.Stream.Length)
                for (var i = 0; i < shipsToAdd.Length; i++)
                {
                    var shipInfos = shipsToAdd[i];
                    if (shipInfos.id != this.id && !_simulation.Environment.VisibleShips.ContainsKey(shipInfos.id))
                    {
                        var ship = new Ship { Id = shipInfos.id, Team = shipInfos.team, X = shipInfos.x, Status = shipInfos.status, Y = shipInfos.y, Rot = shipInfos.rot, Weapons = shipInfos.weapons };
                        //Console.WriteLine("[" + _name + "] Ship {0} added ", shipInfos.id);
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
                //Console.WriteLine("[" + _name + "] Ship {0} removed ", id);
                lock (_simulation.Environment)
                {
                    _simulation.Environment.RemoveShip(id);
                }
            }
        }
    }
}
