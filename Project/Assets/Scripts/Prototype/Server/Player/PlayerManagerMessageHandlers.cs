using UnityEngine;
using iCarus;
using iCarus.Network;
using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype.Server
{
    public sealed partial class PlayerManager
    {
        MessageHandleResult FullUpdateHandler(
            NetConnection connection, 
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Player player = Get(connection);
            if (null == player)
            {
                TSLog.ErrorFormat("player of connection[{0}] doesn't exist", connection.RemoteEndPoint);
                return MessageHandleResult.Finished;
            }

            using (var builder = MessageBuilder.Get())
            {
                var fbb = builder.fbb;
                fbb.Finish(SyncManager.Instance.SampleSnapshot(fbb, true).Value);

                NetOutgoingMessage msg = Server.Instance.netlayer.CreateMessage(MessageID.Msg_SC_Snapshot, fbb);
                player.connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                player.state = Player.State.Playing;
            }
            return MessageHandleResult.Finished;
        }

        MessageHandleResult LoginHandler(
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
                    Msg_CS_Login msg = InstancePool.Get<Msg_CS_Login>();
                    Msg_CS_Login.GetRootAsMsg_CS_Login(byteBuffer, msg);
                    Color color = (new Color()).FromInt(msg.Color);
                    Player newPlayer = Player.New(mIdGen.Alloc(), msg.Name, color, connection);
                    mPlayers.Add(newPlayer);
                    success = true;
                    id = newPlayer.id;
                    TSLog.InfoFormat("new player[{0},{1}]", newPlayer.id, newPlayer.playerName);
                }
            }

            using (var builder = MessageBuilder.Get())
            {
                var fbb = builder.fbb;
                StringOffset errorOffset = default(StringOffset);
                Offset<Msg_SC_UpdatePlayers> playersOffset = default(Offset<Msg_SC_UpdatePlayers>);

                if (success)
                    playersOffset = SyncPlayerList(fbb);
                else
                    errorOffset = fbb.CreateString(error);

                Msg_SC_Login.StartMsg_SC_Login(fbb);
                Msg_SC_Login.AddSuccess(fbb, success);
                if (success)
                    Msg_SC_Login.AddId(fbb, id);
                else
                    Msg_SC_Login.AddError(fbb, errorOffset);
                Msg_SC_Login.AddPlayers(fbb, playersOffset);
                fbb.Finish(Msg_SC_Login.EndMsg_SC_Login(fbb).Value);

                NetOutgoingMessage msg = Server.Instance.netlayer.CreateMessage(MessageID.Msg_SC_Login, fbb);
                connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
            }

            if (success)
            {
                FlatBufferBuilder fbb = new FlatBufferBuilder(64);
                fbb.Finish(SyncNewPlayer(fbb, id).Value);
                foreach (var p in mPlayers)
                {
                    if (p.connection != connection)
                    {
                        NetOutgoingMessage msg = Server.Instance.netlayer.CreateMessage(MessageID.Msg_SC_UpdatePlayers, fbb);
                        p.connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                    }
                }
            }
            return MessageHandleResult.Finished;
        }

        Offset<Msg_SC_UpdatePlayers> SyncPlayerList(FlatBufferBuilder fbb)
        {
            VectorOffset pvOffset = default(VectorOffset);
            if (mPlayers.Count > 0)
            {
                var array = OffsetArrayPool.Alloc<Protocol.Player>(mPlayers.Count);
                foreach (var p in mPlayers)
                {
                    var offset = Protocol.Player.CreatePlayer(fbb, p.id, fbb.CreateString(p.playerName));
                    array.offsets[array.position++] = offset;
                }
                Msg_SC_UpdatePlayers.StartAddPlayersVector(fbb, array.position);
                pvOffset = Helpers.SetVector(fbb, array);
                OffsetArrayPool.Dealloc(ref array);
            }
            return Msg_SC_UpdatePlayers.CreateMsg_SC_UpdatePlayers(fbb, true, pvOffset);
        }

        Offset<Msg_SC_UpdatePlayers> SyncNewPlayer(FlatBufferBuilder fbb, int id)
        {
            VectorOffset pvOffset = default(VectorOffset);
            Player p = Get(id);
            if (null != p)
            {
                var array = OffsetArrayPool.Alloc<Protocol.Player>(1);
                var offset = Protocol.Player.CreatePlayer(fbb, p.id, fbb.CreateString(p.playerName));
                array.offsets[array.position++] = offset;
                Msg_SC_UpdatePlayers.StartAddPlayersVector(fbb, array.position);
                pvOffset = Helpers.SetVector(fbb, array);
                OffsetArrayPool.Dealloc(ref array);
            }
            return Msg_SC_UpdatePlayers.CreateMsg_SC_UpdatePlayers(fbb, false, pvOffset);
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