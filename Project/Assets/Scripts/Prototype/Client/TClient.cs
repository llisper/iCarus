using UnityEngine;

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

        public float serverTickrate { get; private set; }
        public float serverUpdaterate { get; private set; }
        public float tickrate { get; private set; }
        public float cmdrate { get; private set; }

        public void StartClient()
        {
            serverTickrate = AppConfig.Instance.server.tickrate;
            serverUpdaterate = AppConfig.Instance.server.updaterate;
            tickrate = AppConfig.Instance.client.tickrate;
            cmdrate = AppConfig.Instance.client.cmdrate;
            mCmdOverTick = (uint)Mathf.FloorToInt(cmdrate / tickrate);

            mSyncManager.Init();

            var prefab = Resources.Load("Prototype/Cube");
            GameObject go = (GameObject)Instantiate(prefab, transform);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            MovingSphereClient msc = go.AddComponent<MovingSphereClient>();
            mSyncManager.AddTickObject(msc);

            UdpConnector.Configuration netConfig = new UdpConnector.Configuration()
            {
                host = AppConfig.Instance.pacMan.host,
                port = AppConfig.Instance.pacMan.port,
                appIdentifier = AppConfig.Instance.pacMan.appIdentifier,
                onNetStatusChanged = OnNetStatusChanged,
            };
            mConnector.Start(netConfig);
            mConnector.Connect(playerName);
            TCLog.Info("Connecting");

            mConnector.dispatcher.Subscribe(MessageID.Snapshot, SnapshotHandler);
        }

        void Awake()
        {
            mSyncManager = gameObject.AddComponent<SyncManager>();
        }

        void Update()
        {
            mConnector.Update();
        }

        void OnDestroy()
        {
            mConnector.Stop();
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
                mSyncManager.AddDelta(byteBuffer);
                return MessageHandleResult.Processing;
            }
            // TCLog.InfoFormat("snapshot delta -> {0} bytes, [{1},{2}) ticks", byteBuffer.Length, ss.TickStart, ss.TickStart + ss.TickNum);
        }

        uint mCmdOverTick;
        UdpConnector mConnector = new UdpConnector();
        SyncManager mSyncManager;
    }
}
