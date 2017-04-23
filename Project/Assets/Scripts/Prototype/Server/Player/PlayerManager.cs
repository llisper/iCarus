using UnityEngine;
using System;
using System.Collections.Generic;
using iCarus.Singleton;
using Lidgren.Network;
using Foundation;
using FlatBuffers;
using iCarus.Network;
using Protocol;

namespace Prototype
{
    public sealed class PlayerManager : SingletonBehaviour<PlayerManager>
    {
        public float authTimeout { get { return AppConfig.Instance.authTimeout; } }
        public float playerlinger { get { return AppConfig.Instance.playerLinger; } }
        public List<Player> players { get { return mPlayers; } }

        public void Initialize()
        {
            mIdGen = new IdGen(IdRange.players);
            var dispatcher = Server.Instance.netlayer.dispatcher;
            dispatcher.Subscribe(MessageID.Msg_CS_InputDataArray, InputDataArrayHandler);
            dispatcher.Subscribe(MessageID.Msg_CS_NetIdentity, NetIdentityHandler);
            dispatcher.Subscribe(MessageID.Msg_CS_FullUpdate, FullUpdateHandler);
        }

        public void CFixedUpdate()
        {
            foreach (var player in mPlayers)
                player.CFixedUpdate();
        }

        public void OnNewConnection(NetConnection connection)
        {
            mAuthingConnections.Add(new AuthingConnection()
            {
                timer = authTimeout,
                connection = connection,
            });
            TSLog.InfoFormat("incoming connection:{0}", connection.RemoteEndPoint);
        }

        public void OnConnectionStatusChanged(NetConnection connection, string reason)
        {
            /*
            if (connection.Status > NetConnectionStatus.Connected)
            {
                int index = mPlayers.FindIndex(p => p.connection == connection);
                if (-1 != index)
                {
                    mTickObjects.Remove(mPlayers[index]);
                    mPlayers[index].Destroy();
                    mPlayers.RemoveAt(index);
                }
            }
            TSLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
            */
        }

        class AuthingConnection
        {
            public float timer;
            public NetConnection connection;
        }

        List<AuthingConnection> mAuthingConnections = new List<AuthingConnection>();
        List<Player> mPlayers = new List<Player>();
        IdGen mIdGen;

        void Update()
        {
            for (int i = 0; i < mAuthingConnections.Count; )
            {
                AuthingConnection uc = mAuthingConnections[i];
                if ((uc.timer -= Time.deltaTime) <= 0f)
                {
                    TSLog.InfoFormat("remove auth timeout connection:{0}", uc.connection.RemoteEndPoint);
                    uc.connection.Disconnect("auth timeout");
                    mAuthingConnections.RemoveAt(i);
                }
                else
                {
                    ++i;
                }
            }
        }

        Player Get(NetConnection connection)
        {
            foreach (var p in mPlayers)
            {
                if (p.connection == connection)
                    return p;
            }
            return null;
        }

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
