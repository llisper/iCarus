using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using Protocol;
using Foundation;
using Lidgren.Network;
using Prototype.Common;

namespace Prototype.Server
{
    public sealed partial class PlayerManager : SingletonBehaviour<PlayerManager>
    {
        public float authTimeout { get { return AppConfig.Instance.authTimeout; } }
        public float playerlinger { get { return AppConfig.Instance.playerLinger; } }
        public List<Player> players { get { return mPlayers; } }

        internal class AuthingConnection
        {
            public float timer;
            public NetConnection connection;
        }

        internal List<AuthingConnection> mAuthingConnections = new List<AuthingConnection>();
        internal List<Player> mPlayers = new List<Player>();
        internal IdGen mIdGen;

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
    }
}
