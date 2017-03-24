using UnityEngine;
using System.Collections.Generic;
using Foundation;
using iCarus.Network;
using iCarus.Singleton;
using Lidgren.Network;
using Protocol;
using FlatBuffers;

namespace PacMan
{
    public class TestServer : SingletonBehaviour<TestServer>
    {
        public class TPlayer
        {
            public NetConnection connection;
            public bool requestFullSnapshot = false;
            public TPlayer(NetConnection conn) { connection = conn; }
        }

        public float tickrate { get; private set; }
        public float updaterate { get; private set; }

        public int tickCount { get { return mTickCount; } }
        public List<ITickObject> tickObjects { get { return mTickObjects; } }

        public void StartServer()
        {
            tickrate = AppConfig.Instance.server.tickrate;
            updaterate = AppConfig.Instance.server.updaterate;
            mSnapshotOverTick = Mathf.FloorToInt(updaterate / tickrate);

            Time.fixedDeltaTime = tickrate;

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
                var delta = MessageBuilder.Lock();
                var boxArray = OffsetArrayPool.Alloc<TickObjectBox>(mTickObjects.Count);
                foreach (ITickObject to in mTickObjects)
                {
                    boxArray.offsets[boxArray.position++] = TickObjectBox.CreateTickObjectBox(
                        delta,
                        to.type,
                        to.SnapshotDelta(delta));
                }

                SnapshotDelta.StartTickObjectVector(delta, boxArray.position);
                var vecOffset = Helpers.SetVector(delta, boxArray);
                delta.Finish(SnapshotDelta.CreateSnapshotDelta(delta, vecOffset).Value);

                foreach (var p in mPlayers)
                {
                    mUdpListener.SendMessage(
                        MessageID.SnapshotDelta,
                        delta,
                        p.connection,
                        NetDeliveryMethod.UnreliableSequenced);
                }
                OffsetArrayPool.Dealloc(ref boxArray);
                MessageBuilder.Unlock();
            }

            foreach (ITickObject to in mTickObjects)
                to.SimulateFixedUpdate();
            ++mTickCount;
        }

        bool OnIncommingConnection(NetConnection connection, string name, out string denyReason)
        {
            denyReason = null;
            GameLog.InfoFormat("Incomming connection {0} - {1}", connection.RemoteEndPoint, name);
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
            GameLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
        }

        int mTickCount = 0;
        int mSnapshotOverTick;
        bool mRunning = false;
        UdpListener mUdpListener = new UdpListener();
        List<TPlayer> mPlayers = new List<TPlayer>();
        List<ITickObject> mTickObjects = new List<ITickObject>();
    }
}
