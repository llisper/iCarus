using iCarus;
using iCarus.Network;
using Protocol;
using FlatBuffers;
using Prototype.Game;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class Login : GameState
    {
        public override void Start()
        {
            game.netlayer.onNetStatusChanged += MonitorNetwork;
            game.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_Login, LoginHandler);
            StartLogin();
        }

        protected override void Update() { } 
        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= MonitorNetwork;
            game.netlayer.dispatcher.Unsubscribe(MessageID.Msg_SC_Login, LoginHandler);
        }

        void StartLogin()
        {
            using (var builder = MessageBuilder.Get())
            {
                FlatBufferBuilder fbb = builder.fbb;
                var nameOffset = fbb.CreateString(game.playerName);
                Msg_CS_Login.StartMsg_CS_Login(fbb);
                Msg_CS_Login.AddName(fbb, nameOffset);
                Msg_CS_Login.AddColor(fbb, UnityEngine.Random.ColorHSV().ToInt());
                fbb.Finish(Msg_CS_Login.EndMsg_CS_Login(fbb).Value);
                var msg = game.netlayer.CreateMessage(MessageID.Msg_CS_Login, fbb);
                game.netlayer.SendMessage(msg, NetDeliveryMethod.ReliableOrdered);
            }
        }

        MessageHandleResult LoginHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_Login msg = InstancePool.Get<Msg_SC_Login>();
            Msg_SC_Login.GetRootAsMsg_SC_Login(byteBuffer, msg);
            if (!msg.Success)
            {
                GameStateLog.Error("Login failed:" + msg.Error);
                TransitTo<Error>(msg.Error);
            }
            else
            {
                game.id = msg.Id;
                GameStateLog.Info("Login success, game id:" + msg.Id);

                Msg_SC_UpdatePlayers updateplayers = InstancePool.Get<Msg_SC_UpdatePlayers>();
                msg.GetPlayers(updateplayers);
                PlayerManagerClient.Instance.UpdatePlayers(updateplayers);
                GameStateLog.Info("Update players");

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
