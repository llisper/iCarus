using Protocol;
using FlatBuffers;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class InGame : GameState
    {
        public override void Start()
        {
            game.netlayer.onNetStatusChanged += MonitorNetwork;
            game.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_Snapshot, SnapshotHandler);
        }

        protected override void Update()
        {
            
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            InputManager.Instance.UpdateInput(game.netlayer);
            SyncManagerClient.Instance.SimulateFixedUpdate();
        }

        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= MonitorNetwork;
            game.netlayer.dispatcher.Unsubscribe(MessageID.Msg_SC_Snapshot, SnapshotHandler);
        }

        MessageHandleResult SnapshotHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_Snapshot snapshot = InstancePool.Get<Msg_SC_Snapshot>();
            Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(byteBuffer, snapshot);
            if (snapshot.Full)
            {
                SyncManagerClient.Instance.FullUpdate(snapshot);
                return MessageHandleResult.Finished;
            }
            else
            {
                SyncManagerClient.Instance.AddDelta(snapshot.TickNow, byteBuffer, message);
                return MessageHandleResult.Processing;
            }
        }

        void MonitorNetwork(UdpConnector netlayer, NetConnectionStatus status, string reason)
        {
            if (ConnectionLost(status))
            {
                GameStateLog.ErrorFormat("net status:{0}, reason:{1}", status, reason);
                TransitTo<Reconnect>();
            }
        }
    }
}
