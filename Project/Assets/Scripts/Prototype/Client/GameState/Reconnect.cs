using UnityEngine;
using Protocol;
using FlatBuffers;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype.GameState
{
    public class Reconnect : GameState
    {
        public override void Start()
        {
            game.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_Reconnect, ReconnectHandler);
        }

        protected override void Update()
        {
            if (game.netlayer.connectionStatus != NetConnectionStatus.Connected &&
                (mTimer -= Time.deltaTime) <= 0f)
            {
                mTimer = 5f;
                if (mAttempts++ >= 3)
                {
                    TransitTo<Error>("reconnect timeout");
                    return;
                }
                ConnectAttempt();
                GameStateLog.Info("reconnect attempt #" + mAttempts);
            }
        }

        protected override void Destroy()
        {
            game.netlayer.onNetStatusChanged -= OnNetStatusChanged;
            game.netlayer.dispatcher.Unsubscribe(MessageID.Msg_SC_Reconnect, ReconnectHandler);
        }

        int mAttempts;
        float mTimer;

        void ConnectAttempt()
        {
            game.netlayer.Stop();
            var config = Connect.BuildConfiguration(OnNetStatusChanged);
            game.netlayer.Start(config);
            NetOutgoingMessage hail = game.netlayer.CreateMessage();
            hail.Write(game.id);
            game.netlayer.Connect(hail);
        }

        MessageHandleResult ReconnectHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_Reconnect msg = InstancePool.Get<Msg_SC_Reconnect>();
            Msg_SC_Reconnect.GetRootAsMsg_SC_Reconnect(byteBuffer, msg);
            if (msg.Success)
                TransitTo<DataFullUpdate>();
            else
                TransitTo<VerifyIdentity>();
            return MessageHandleResult.Finished;
        }

        void OnNetStatusChanged(UdpConnector netlayer, NetConnectionStatus status, string reason)
        {
            GameStateLog.Info("reconnect net status:" + status);
        }
    }
}
