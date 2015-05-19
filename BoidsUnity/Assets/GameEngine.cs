using UnityEngine;
using System.Collections;
using Models;
using Stormancer;
using System.Threading.Tasks;
using Stormancer.Core;
using System.IO;
using System.Collections.Generic;
using System;

public class GameEngine : MonoBehaviour
{
    private Scene _scene;

    private readonly Dictionary<ushort, Ship> _gameObjects = new Dictionary<ushort, Ship>();

    public GameObject ShipPrefab;

    // Use this for initialization
    void Start()
    {
        UniRx.MainThreadDispatcher.Initialize();
        var ui = GameObject.Find("UI");
        var config = Stormancer.ClientConfiguration.ForAccount("d81fc876-6094-3d92-a3d0-86d42d866b96", "boids-demo");

        var client = new Stormancer.Client(config);



        client.GetPublicScene("main-session", new PlayerInfos { isObserver = true }).Then(
            scene =>
            {
                _scene = scene;
                scene.AddRoute("position.update", OnPositionUpdate);
                scene.AddRoute("ship.remove", OnShipRemoved);
                scene.AddRoute("ship.add", OnShipAdded);


                _scene.Connect().Then(() =>
                {
                    UniRx.MainThreadDispatcher.Post(() =>
                    {
                        ui.SetActive(false);//Hide loading ui.


                    });

                });
            }
            );
    }

    // Update is called once per frame
    void Update()
    {
        var deleteArray = new List<Ship>();
        foreach (var ship in _gameObjects.Values)
        {
            if(ship.Deletable)
            {
                deleteArray.Add(ship);
            }
            else if (ship.Obj == null)
            {
                ship.Obj = (GameObject)Instantiate(ShipPrefab, new Vector3(ship.TargetX, ship.TargetY),  Quaternion.Euler(0, 0, ship.TargetRot * (180 / (float)Math.PI)));
            }
            else
            {
                var ratio = (DateTime.UtcNow - ship.LastDate).TotalMilliseconds / (ship.TargetDate - ship.LastDate).TotalMilliseconds;

                var position = Vector3.Lerp(new Vector3(ship.LastX, ship.LastY), new Vector3(ship.TargetX, ship.TargetY), (float)ratio);
                ship.Obj.transform.position = position;
                var rotation = Quaternion.Lerp(Quaternion.Euler(0, 0, ship.LastRot * (180 / (float)Math.PI)), Quaternion.Euler(0, 0, ship.TargetRot * (180 / (float)Math.PI)), (float)ratio);
                ship.Obj.transform.rotation = rotation;
            }
        }

        foreach(var el in deleteArray)
        {
            _gameObjects.Remove(el.Id);
            if(el.Obj!=null)
            {
                GameObject.Destroy(el.Obj);
            }
        }
    }

    private void OnShipAdded(Packet<IScenePeer> obj)
    {

        var shipInfos = obj.ReadObject<ShipCreatedDto>();

        var ship = new Ship { Id = shipInfos.id, TargetX = shipInfos.x, LastX = shipInfos.x, LastY = shipInfos.y, TargetY = shipInfos.y, TargetRot = shipInfos.rot, TargetDate = DateTime.UtcNow };
        UnityEngine.Debug.Log(string.Format("Ship {0} added ", shipInfos.id));

        _gameObjects.Add(ship.Id, ship);

    }

    private void OnShipRemoved(Packet<IScenePeer> obj)
    {


        var id = obj.ReadObject<ushort>();
        UnityEngine.Debug.Log(string.Format("Ship {0} removed ", id));
        Ship ship;
        if(_gameObjects.TryGetValue(id,out ship))
        {
            ship.Deletable = true;
        }


    }

    private void OnPositionUpdate(Packet<IScenePeer> obj)
    {

        using (var reader = new BinaryReader(obj.Stream))
        {
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var id = reader.ReadUInt16();
                var x = reader.ReadSingle();
                var y = reader.ReadSingle();
                var rot = reader.ReadSingle();

                Ship ship;
                if (!_gameObjects.TryGetValue(id, out ship))
                {
                    ship = new Ship { Id = id };
                    _gameObjects.Add(id, ship);
                }
                ship.LastX = ship.TargetX;
                ship.LastY = ship.TargetY;
                ship.LastRot = ship.TargetRot;
                ship.LastDate = ship.TargetDate;

                ship.TargetX = x;
                ship.TargetY = y;
                ship.TargetRot = rot;
                ship.TargetDate = DateTime.UtcNow + TimeSpan.FromMilliseconds(100);
                

            }
        }
    }



    private class Ship
    {
        public ushort Id { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public float LastX { get; set; }
        public float LastY { get; set; }

        public DateTime TargetDate { get; set; }
        public DateTime LastDate { get; set; }
        public float TargetRot { get; set; }

        public GameObject Obj { get; set; }
        public bool Deletable { get; internal set; }
        public float LastRot { get; internal set; }
    }
}

