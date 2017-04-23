using System;
using Protocol;
using FlatBuffers;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class DataFullUpdate : GameState
    {
        public override void Start()
        {
            game.netlayer.onNetStatusChanged += MonitorNetwork;
            game.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_Snapshot, FullUpdateHandler);
            FullUpdateRequest();
        }

        protected override void Update()
        {
        }

        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= MonitorNetwork;
            game.netlayer.dispatcher.Unsubscribe(MessageID.Msg_SC_Snapshot, FullUpdateHandler);
        }

        void FullUpdateRequest()
        {
            using (var builder = MessageBuilder.Get())
            {
                FlatBufferBuilder fbb = builder.fbb;
                Msg_CS_FullUpdate.StartMsg_CS_FullUpdate(fbb);
                fbb.Finish(Msg_CS_FullUpdate.EndMsg_CS_FullUpdate(fbb).Value);
                var msg = game.netlayer.CreateMessage(MessageID.Msg_CS_FullUpdate, fbb);
                game.netlayer.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            }
            GameStateLog.Info("full update request");
        }

        MessageHandleResult FullUpdateHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_Snapshot snapshot = InstancePool.Get<Msg_SC_Snapshot>();
            Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(byteBuffer, snapshot);
            if (snapshot.Full)
            {
                SyncManagerClient.Instance.FullUpdate(snapshot);
                GameStateLog.Info("apply full update");
                TransitTo<InGame>();
            }
            else
            {
                string error = "server reply full update request with a delta snapshot";
                GameStateLog.Error(error);
                TransitTo<Error>(error);
            }
            return MessageHandleResult.Finished;
        }

        void MonitorNetwork(UdpConnector netlayer, NetConnectionStatus status, string reason)
        {
            if (ConnectionLost(status))
            {
                GameStateLog.ErrorFormat("net status:{0}, reason:{1}", status, reason);
                TransitTo<Error>(reason);
            }
        }
    }
}
