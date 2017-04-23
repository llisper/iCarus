using iCarus.Network;
using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public sealed partial class PlayerManager
    {
        MessageHandleResult FullUpdateHandler(
            NetConnection connection, 
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Player player = Get(connection);
            if (null != player)
                player.state = Player.State.FullSync;
            return MessageHandleResult.Finished;
        }

        MessageHandleResult NetIdentityHandler(
            NetConnection connection, 
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            bool success = false;
            string error = null;
            int id = 0;

            Player player = Get(connection);
            if (null != player)
            {
                error = string.Format("player[{0},{1}] has already been auth", player.id, player.playerName);
                TSLog.ErrorFormat(error);
            }
            else
            {
                int index = mAuthingConnections.FindIndex(v => v.connection == connection);
                if (index < 0)
                {
                    error = string.Format("connection[{0}] is not found in waiting list", connection.RemoteEndPoint);
                    TSLog.ErrorFormat(error);
                }
                else
                {
                    mAuthingConnections.RemoveAt(index);
                    Msg_CS_NetIdentity msg = InstancePool.Get<Msg_CS_NetIdentity>();
                    Msg_CS_NetIdentity.GetRootAsMsg_CS_NetIdentity(byteBuffer, msg);
                    Player newPlayer = Player.New(mIdGen.Alloc(), msg.Name, connection);
                    mPlayers.Add(newPlayer);
                    success = true;
                    id = newPlayer.id;
                    TSLog.InfoFormat("new player[{0},{1}]", newPlayer.id, newPlayer.playerName);
                }
            }

            using (var builder = MessageBuilder.Get())
            {
                var fbb = builder.fbb;
                var errorOffset = !success ? fbb.CreateString(error) : default(StringOffset);
                Msg_SC_NetIdentity.StartMsg_SC_NetIdentity(fbb);
                Msg_SC_NetIdentity.AddSuccess(fbb, success);
                if (success)
                    Msg_SC_NetIdentity.AddId(fbb, id);
                else
                    Msg_SC_NetIdentity.AddError(fbb, errorOffset);
                fbb.Finish(Msg_SC_NetIdentity.EndMsg_SC_NetIdentity(fbb).Value);

                NetOutgoingMessage msg = Server.Instance.netlayer.CreateMessage(MessageID.Msg_SC_NetIdentity, fbb);
                connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
            }
            return MessageHandleResult.Finished;
        }

        MessageHandleResult InputDataArrayHandler(
            NetConnection connection, 
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Player player = Get(connection);
            if (null != player)
            {
                Msg_CS_InputDataArray ida = InstancePool.Get<Msg_CS_InputDataArray>();
                Msg_CS_InputDataArray.GetRootAsMsg_CS_InputDataArray(byteBuffer, ida);
                player.UpdateInput(ida);
            }
            return MessageHandleResult.Finished;
        }
    }
}