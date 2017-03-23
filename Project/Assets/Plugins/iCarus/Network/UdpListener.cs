using System;
using System.Net;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace iCarus.Network
{
    public class NetLog : Log.Logging.Define<NetLog> { }
    public class NetworkException : Exception { }

    public class UdpListener
    {
        public IPAddress localAddress { get { return mConfig.localAddress; } }
        public int port { get { return mConfig.port; } }
        public string appIdentifier { get { return mConfig.appIdentifier; } }
        public MessageDispatcher dispatcher { get { return mDispatcher; } }

        public delegate bool OnIncommingConnection(NetConnection connection, string name, out string denyReason);
        public delegate void OnConnectionStatusChanged(NetConnection connection, string reason);

        public class Configuration
        {
            public IPAddress localAddress = IPAddress.Any;
            public int port = 65534;
            public string appIdentifier = "UnknownApp";
            public int maxConnections = 4;
            public int defaultOutgoingMessageCapacity = 16384;
            public OnIncommingConnection onIncommingConnection;
            public OnConnectionStatusChanged onConnectionStatusChanged;
        }

        public void Start(Configuration config)
        {
            mConfig = config;

            NetPeerConfiguration netConfig = new NetPeerConfiguration(appIdentifier)
            {
                LocalAddress = localAddress,
                Port = port,
                MaximumConnections = mConfig.maxConnections,
                DefaultOutgoingMessageCapacity = mConfig.defaultOutgoingMessageCapacity,
            };
            netConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            #if VERBOSE_DEBUG
            netConfig.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            #endif

            mServer = new NetServer(netConfig);
            mServer.Start();

            NetLog.InfoFormat("{0} Network Initialized", appIdentifier);
        }

        public void Stop()
        {
            if (null != mServer)
            {
                mServer.Shutdown("Closed");
                mServer = null;
            }
        }

        public void Update()
        {
            if (null == mServer)
                return;

            NetIncomingMessage message;
            while (null != (message = mServer.ReadMessage()))
            {
                try
                {
                    switch (message.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.ErrorMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            {
                                HandleDebugMessage(message);
                                break;
                            }
                        case NetIncomingMessageType.ConnectionApproval:
                            {
                                HandleConnectionApproval(message);
                                break;
                            }
                        case NetIncomingMessageType.StatusChanged:
                            {
                                HandleStatusChanged(message);
                                break;
                            }
                        case NetIncomingMessageType.Data:
                            {
                                HandleData(message);
                                break;
                            }
                        default:
                            {
                                NetLog.WarnFormat(
                                    "{0} Unhandled message type:{1}, bytes:{2}",
                                    appIdentifier,
                                    message.MessageType,
                                    message.LengthBytes);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    NetLog.Exception(appIdentifier, e);
                }
                mServer.Recycle(message);
            }
        }

        void HandleDebugMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                    {
                        NetLog.DebugFormat("{0} {1}", appIdentifier, message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.ErrorMessage:
                    {
                        NetLog.ErrorFormat("{0}, {1}", appIdentifier, message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.WarningMessage:
                    {
                        NetLog.WarnFormat("{0}, {1}", appIdentifier, message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.VerboseDebugMessage:
                    {
                        NetLog.DebugFormat("{0}, {1}", appIdentifier, message.ReadString());
                        break;
                    }
            }
        }

        void HandleConnectionApproval(NetIncomingMessage message)
        {
            string name = message.ReadString();

            bool accept = false;
            string denyReason = String.Empty;
            NetConnection conn = message.SenderConnection;
            if (null != mConfig.onIncommingConnection)
                accept = mConfig.onIncommingConnection(conn, name, out denyReason);

            if (accept)
            {
                conn.Approve();
                NetLog.DebugFormat("{0} Approve connection {1}", appIdentifier, conn.RemoteEndPoint);
            }
            else
            {
                message.SenderConnection.Deny(denyReason);
                NetLog.DebugFormat(
                    "{0} Deny connection {1}, {2}",
                    appIdentifier,
                    conn.RemoteEndPoint,
                    denyReason);
            }
        }

        void HandleStatusChanged(NetIncomingMessage message)
        {
            NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
            string reason = message.ReadString();
            if (null != mConfig.onConnectionStatusChanged)
                mConfig.onConnectionStatusChanged(message.SenderConnection, reason);

            NetLog.DebugFormat(
                "{0} Connection {1} status changed: {2}:{3}", 
                appIdentifier,
                message.SenderEndPoint,
                status,
                reason);
        }

        void HandleData(NetIncomingMessage message)
        {
            MessageID id = (MessageID)message.ReadUInt16();
            ushort len = message.ReadUInt16();
            ByteBuffer byteBuffer = ByteBufferPool.Alloc(len);
            try
            {
                message.ReadBytes(byteBuffer.Data, 0, len);
                dispatcher.Fire(id, byteBuffer);
            }
            finally
            {
                ByteBufferPool.Dealloc(ref byteBuffer);
            }
        }

        internal NetOutgoingMessage CreateMessage()
        {
            return mServer.CreateMessage();
        }

        NetServer mServer;
        Configuration mConfig;
        MessageDispatcher mDispatcher = new MessageDispatcher();
    }
}
