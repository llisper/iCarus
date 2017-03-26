using UnityEngine;

using System.Collections.Generic;

using iCarus.Log;
using iCarus.Network;
using iCarus.Singleton;
using Protocol;
using Foundation;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public class TPlayer
    {
        public NetConnection connection;
        public bool requestFullSnapshot = true;
        public TPlayer(NetConnection conn) { connection = conn; }
    }

    public class TSLog : Logging.Define<TSLog> { }
    public sealed class TServer : SingletonBehaviour<TServer>
    {
        public float tickrate { get; private set; }
        public float updaterate { get; private set; }
        public uint tickCount { get { return mTickCount; } }
        public uint snapshotOverTick { get { return mSnapshotOverTick; } }
        public List<ITickObject> tickObjects { get { return mTickObjects; } }

        public void StartServer()
        {
            tickrate = AppConfig.Instance.server.tickrate;
            updaterate = AppConfig.Instance.server.updaterate;
            mSnapshotOverTick = (uint)Mathf.FloorToInt(updaterate / tickrate);
            TSLog.InfoFormat("tickrate:{0}, updaterate:{1}, ss/t:{2}", tickrate, updaterate, mSnapshotOverTick);

            Time.fixedDeltaTime = tickrate;
            var prefab = Resources.Load("Prototype/Cube");
            GameObject go = (GameObject)Instantiate(prefab, transform);
            go.transform.position = new Vector3(3f, 0f, 0f);
            go.transform.rotation = Quaternion.identity;
            mTickObjects.Add(go.AddComponent<MovingSphere>());

            UdpListener.Configuration netConfig = new UdpListener.Configuration()
            {
                appIdentifier = AppConfig.Instance.pacMan.appIdentifier,
                port = AppConfig.Instance.pacMan.port,
                maxConnections = AppConfig.Instance.pacMan.maxConnection,
                onIncommingConnection = OnIncommingConnection,
                onConnectionStatusChanged = OnConnectionStatusChanged,
            };
            mUdpListener.Start(netConfig);
            mRunning = true;
            TSLog.Info("Start Running");
        }

        void Update()
        {
            if (mRunning)
                mUdpListener.Update();
        }

        void OnDestroy()
        {
            mUdpListener.Stop();
        }

        void FixedUpdate()
        {
            if (!mRunning) return;

            if (mTickCount > 0 && (mTickCount % mSnapshotOverTick) == 0)
            {
                foreach (var p in mPlayers)
                {
                    if (p.requestFullSnapshot)
                    {
                        mPlayersToSendFull.Add(p);
                        p.requestFullSnapshot = false;
                    }
                    else
                    {
                        mPlayersToSendDelta.Add(p);
                    }
                }

                SendSnapshotToPlayers(mPlayersToSendDelta, false);
                SendSnapshotToPlayers(mPlayersToSendFull, true);
                mPlayersToSendDelta.Clear();
                mPlayersToSendFull.Clear();
            }

            foreach (ITickObject to in mTickObjects)
                to.SimulateFixedUpdate();
            ++mTickCount;
        }

        void SendSnapshotToPlayers(List<TPlayer> players, bool full)
        {
            if (players.Count <= 0)
                return;

            var fbb = MessageBuilder.Lock();
            var boxArray = OffsetArrayPool.Alloc<TickObjectBox>(mTickObjects.Count);
            foreach (ITickObject to in mTickObjects)
            {
                boxArray.offsets[boxArray.position++] = TickObjectBox.CreateTickObjectBox(
                    fbb, to.id, to.type, to.Snapshot(fbb, full));
            }

            Snapshot.StartTickObjectVector(fbb, boxArray.position);
            var vecOffset = Helpers.SetVector(fbb, boxArray);
            fbb.Finish(Snapshot.CreateSnapshot(
                fbb,
                mTickCount,
                mSnapshotOverTick,
                full,
                vecOffset).Value);

            foreach (var p in players)
            {
                NetOutgoingMessage msg = mUdpListener.CreateMessage(MessageID.Snapshot, fbb);
                // msg.Write(...); // latest player input applied on server
                mUdpListener.SendMessage(
                    msg, p.connection, 
                    full ? NetDeliveryMethod.ReliableOrdered : NetDeliveryMethod.UnreliableSequenced);
            }
            OffsetArrayPool.Dealloc(ref boxArray);
            MessageBuilder.Unlock();
        }

        bool OnIncommingConnection(NetConnection connection, string name, out string denyReason)
        {
            denyReason = null;
            TSLog.InfoFormat("Incomming connection {0} - {1}", connection.RemoteEndPoint, name);
            mPlayers.Add(new TPlayer(connection));
            return true;
        }

        void OnConnectionStatusChanged(NetConnection connection, string reason)
        {
            if (connection.Status > NetConnectionStatus.Connected)
            {
                int index = mPlayers.FindIndex(p => p.connection == connection);
                if (-1 != index)
                    mPlayers.RemoveAt(index);
            }
            TSLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
        }

        uint mTickCount = 0;
        uint mSnapshotOverTick;
        bool mRunning = false;
        UdpListener mUdpListener = new UdpListener();
        List<TPlayer> mPlayers = new List<TPlayer>();
        List<TPlayer> mPlayersToSendDelta = new List<TPlayer>();
        List<TPlayer> mPlayersToSendFull = new List<TPlayer>();
        List<ITickObject> mTickObjects = new List<ITickObject>();
    }
}
