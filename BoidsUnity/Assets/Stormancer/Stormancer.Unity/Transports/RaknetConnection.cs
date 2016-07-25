﻿using RakNet;
using Stormancer.Core;
using Stormancer.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stormancer.Networking
{

    internal class RakNetConnection : IConnection
    {
        private class RakNetConnectionStatistics : IConnectionStatistics
        {
            public static IConnectionStatistics GetConnectionStatistics(RakNetConnection connection)
            {
                var result = new RakNetConnectionStatistics();
                using (var stats = connection._rakPeer.GetStatistics(connection._rakPeer.GetSystemAddressFromGuid(connection._guid)))
                {
                    result.PacketLossRate = stats.packetlossLastSecond;
                    result.BytesPerSecondLimitationType = stats.isLimitedByOutgoingBandwidthLimit ? BPSLimitationType.OutgoingBandwidth : (stats.isLimitedByCongestionControl ? BPSLimitationType.CongestionControl : BPSLimitationType.None);
                    result.BytesPerSecondLimit = (long)(stats.isLimitedByOutgoingBandwidthLimit ? stats.BPSLimitByOutgoingBandwidthLimit : stats.BPSLimitByCongestionControl);
                    result._queuedBytes = stats.bytesInSendBuffer.ToArray();
                    result._queuedPackets = stats.messageInSendBuffer.ToArray();
                }

                return result;
            }

            private RakNetConnectionStatistics()
            {
            }
            public float PacketLossRate { get; private set; }

            public BPSLimitationType BytesPerSecondLimitationType { get; private set; }

            public long BytesPerSecondLimit { get; private set; }

            private double[] _queuedBytes;
            public double QueuedBytes
            {
                get
                {
                    return this._queuedBytes.Sum();
                }
            }

            public double QueuedBytesForPriority(Core.PacketPriority priority)
            {
                return this._queuedBytes[(int)priority];
            }

            private uint[] _queuedPackets;

            public int QueuedPackets
            {
                get { return this._queuedPackets.Cast<int>().Sum(); }
            }

            public int QueuedPacketsForPriority(Core.PacketPriority priority)
            {
                return (int)(this._queuedPackets[(int)priority]);
            }
        }



        private RakPeerInterface _rakPeer;
        private RakNetGUID _guid;

        private readonly Action<RakNetConnection> _closeAction;
        private readonly Dictionary<string, string> _metadata = new Dictionary<string, string>();
        internal RakNetConnection(RakNetGUID guid, long id, RakPeerInterface peer,
            Action<RakNetConnection> closeAction)
        {
            ConnectionDate = DateTime.UtcNow;
            LastActivityDate = DateTime.UtcNow;
            Id = id;
            _guid = guid;

            _rakPeer = peer;
            _closeAction = closeAction;
            State = Stormancer.Core.ConnectionState.Connecting;
        }

        /// <summary>
        /// Id of the connection
        /// </summary>
        public RakNetGUID Guid
        {
            get
            {
                return _guid;
            }
        }

        /// <summary>
        /// Connection date
        /// </summary>
        public DateTime ConnectionDate { get; internal set; }

        /// <summary>
        /// Last activity 
        /// </summary>
        public DateTime LastActivityDate { get; internal set; }


        /// <summary>
        /// IP address of the connection.
        /// </summary>
        public string IpAddress
        {
            get
            {
                return _rakPeer.GetSystemAddressFromGuid(_guid).ToString();
            }
        }

        /// <summary>
        /// The account id of the application to which this connection is connected.
        /// </summary>
        public string Account { get; private set; }

        /// <summary>
        /// The id of the application to which this connection is connected.
        /// </summary>
        public string Application { get; private set; }

        public Stormancer.Core.ConnectionState State { get; private set; }


        public long Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns an hashcode based on Id.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Equals implementation for IConnection
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var v2 = obj as IConnection;
            if (v2 == null)
            {
                return false;
            }
            else
            {
                return v2.Id == this.Id;
            }
        }

        /// <summary>
        /// Connection metadata
        /// </summary>
        public Dictionary<string, string> Metadata
        {
            get { return _metadata; }
        }

        /// <summary>
        /// Closes the connection.
        /// </summary>
        public void Close()
        {
            _closeAction(this);
        }

        /// <summary>
        /// Sends a system request to the remote peer.
        /// </summary>
        /// <param name="msgId">Id of the system message</param>
        /// <param name="writer"></param>
        public void SendSystem(byte msgId, Action<System.IO.Stream> writer)
        {
            this.SendSystem(msgId, writer, Stormancer.Core.PacketPriority.MEDIUM_PRIORITY);
        }

        /// <summary>
        /// Sends a system request to the remote peer.
        /// </summary>
        /// <param name="msgId">Id of the system message</param>
        /// <param name="writer"></param>
        /// <param name="priority">The priority of the message to send</param>
        public void SendSystem(byte msgId, Action<System.IO.Stream> writer, Stormancer.Core.PacketPriority priority)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            SendRaw(s =>
            {
                s.WriteByte(msgId);
                writer(s);
            }, priority, Core.PacketReliability.RELIABLE_ORDERED, (char)0);
        }

        public void SendRaw(Action<Stream> writer, Stormancer.Core.PacketPriority priority, Stormancer.Core.PacketReliability reliability, char channel)
        {

            var stream = new BitStream();
            writer(new BSStream(stream));
            var result = _rakPeer.Send(stream, (RakNet.PacketPriority)priority, (RakNet.PacketReliability)reliability, channel, this.Guid, false);

            if (result == 0)
            {
                throw new InvalidOperationException("Failed to send message.");
            }
        }

        /// <summary>
        /// Sends a scene request to the remote peer.
        /// </summary>
        /// <param name="sceneIndex"></param>
        /// <param name="route"></param>
        /// <param name="writer"></param>
        /// <param name="priority"></param>
        /// <param name="reliability"></param>
        /// <param name="channel"></param>
        public void SendToScene(byte sceneIndex, ushort route, Action<System.IO.Stream> writer, Stormancer.Core.PacketPriority priority, Stormancer.Core.PacketReliability reliability)
        {
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }
            var stream = new BitStream();
            var s = new BSStream(stream);
            s.WriteByte(sceneIndex);
            s.Write(BitConverter.GetBytes(route), 0, 2);
            writer(s);
            var result = _rakPeer.Send(stream, (RakNet.PacketPriority)priority, (RakNet.PacketReliability)reliability, (char)0, this.Guid, false);

            if (result == 0)
            {
                throw new InvalidOperationException("Failed to send message.");
            }
        }


        public Action<string> ConnectionClosed
        {
            get;
            set;
        }



        private Dictionary<Type, object> _localData = new Dictionary<Type, object>();


        public T GetComponent<T>()
        {
            object result;
            if (_localData.TryGetValue(typeof(T), out result))
            {
                return (T)result;
            }
            else
            {
                return default(T);
            }
        }

        public void RegisterComponent<T>(T component)
        {
            _localData.Add(typeof(T), component);
        }


        public void SetApplication(string account, string application)
        {
            if (this.Account == null)
            {
                this.Account = account;
                this.Application = application;
            }
        }



        public int Ping
        {
            get { return this._rakPeer.GetLastPing(this._guid); }
        }

        public IConnectionStatistics GetConnectionStatistics()
        {
            return RakNetConnectionStatistics.GetConnectionStatistics(this);
        }

    }
}
