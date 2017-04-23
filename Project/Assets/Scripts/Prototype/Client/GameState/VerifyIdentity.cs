using System;
using Protocol;
using iCarus.Network;
using Lidgren.Network;
using FlatBuffers;

namespace Prototype.GameState
{
    public class VerifyIdentity : GameState
    {
        public override void Start()
        {
            game.netlayer.onNetStatusChanged += MonitorNetwork;
            game.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_NetIdentity, IdentifyHandler);
            SendIdentity();
        }

        protected override void Update() { } 
        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= MonitorNetwork;
            game.netlayer.dispatcher.Unsubscribe(MessageID.Msg_SC_NetIdentity, IdentifyHandler);
        }

        void SendIdentity()
        {
            using (var builder = MessageBuilder.Get())
            {
                FlatBufferBuilder fbb = builder.fbb;
                var nameOffset = fbb.CreateString(game.playerName);
                fbb.Finish(Msg_CS_NetIdentity.CreateMsg_CS_NetIdentity(fbb, nameOffset).Value);
                var msg = game.netlayer.CreateMessage(MessageID.Msg_CS_NetIdentity, fbb);
                game.netlayer.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            }
        }

        MessageHandleResult IdentifyHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_NetIdentity msg = InstancePool.Get<Msg_SC_NetIdentity>();
            Msg_SC_NetIdentity.GetRootAsMsg_SC_NetIdentity(byteBuffer, msg);
            if (!msg.Success)
            {
                GameStateLog.Error("verify identify failed:" + msg.Error);
                TransitTo<Error>(msg.Error);
            }
            else
            {
                game.id = msg.Id;
                GameStateLog.Info("verify identify success, id:" + msg.Id);
                TransitTo<DataFullUpdate>();
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
