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
    public class TCLog : Logging.Define<TCLog> { }
    public sealed class TClient : SingletonBehaviour<TClient>
    {
        public string playerName = "anonymous";

        public float serverUpdaterate { get; private set; }
        public float tickrate { get; private set; }
        public float cmdrate { get; private set; }
        public uint snapshotOverTick { get; private set; }
        public NetClient netClient { get { return mConnector.netClient; } }
        public TInput input { get { return mInput; } }

        public void StartClient()
        {
            serverUpdaterate = AppConfig.Instance.updaterate;
            tickrate = AppConfig.Instance.tickrate;
            cmdrate = AppConfig.Instance.cmdrate;
            snapshotOverTick = (uint)Mathf.FloorToInt(serverUpdaterate / tickrate);

            Time.fixedDeltaTime = tickrate;
            mSyncManager.Init();
            mInput.Init();

            var prefab = Resources.Load("Prototype/Cube");
            GameObject go = (GameObject)Instantiate(prefab, transform);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            MovingSphereClient msc = go.AddComponent<MovingSphereClient>();
            mSyncManager.AddTickObject(msc);

            TPlayerClient player = new TPlayerClient();
            player.Init();
            mSyncManager.AddTickObject(player);

            UdpConnector.Configuration netConfig = new UdpConnector.Configuration()
            {
                host = AppConfig.Instance.pacMan.host,
                port = AppConfig.Instance.pacMan.port,
                netPeerConfig = new NetPeerConfiguration(AppConfig.Instance.pacMan.appIdentifier)
                {
                    DefaultOutgoingMessageCapacity = 10240,
                    SimulatedDuplicatesChance = AppConfig.Instance.simulatedDuplicatesChance,
                    SimulatedLoss = AppConfig.Instance.simulatedLoss,
                    SimulatedMinimumLatency = AppConfig.Instance.simulatedMinimumLatency,
                    SimulatedRandomLatency = AppConfig.Instance.simulatedRandomLatency,
                },
                onNetStatusChanged = OnNetStatusChanged,
            };
            mConnector.Start(netConfig);
            mConnector.Connect(playerName);
            TCLog.Info("Connecting");

            mConnector.dispatcher.Subscribe(MessageID.Snapshot, SnapshotHandler);
        }

        void Update()
        {
            mConnector.Update();
        }

        void FixedUpdate()
        {
            if (mConnector.connectionStatus == NetConnectionStatus.Connected)
                mInput.UpdateInput(mConnector);
            mSyncManager.SimulateFixedUpdate();
        }

        void OnDestroy()
        {
            mConnector.Stop();
        }

        void OnDrawGizmosSelected()
        {
            mInput.DrawGizmosSelected();
        }

        void OnNetStatusChanged(UdpConnector connector, NetConnectionStatus status, string reason)
        {
            TCLog.InfoFormat("Connection status changed {0} {1}", status, reason);
        }

        MessageHandleResult SnapshotHandler(NetConnection connection, ByteBuffer byteBuffer, NetIncomingMessage message)
        {
            Snapshot ss = InstancePool.Get<Snapshot>();
            Snapshot.GetRootAsSnapshot(byteBuffer, ss);
            if (ss.Full)
            {
                mSyncManager.FullUpdate(ss);
                return MessageHandleResult.Finished;
            }
            else
            {
                mSyncManager.AddDelta(ss.TickNow, byteBuffer, message);
                return MessageHandleResult.Processing;
            }
        }

        UdpConnector mConnector = new UdpConnector();
        TInput mInput = new TInput();
        SyncManager mSyncManager = new SyncManager();
    }
}
