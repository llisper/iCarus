﻿using System;
using System.Net;
using System.Collections.Generic;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace iCarus.Network
{
    public class NetLog : Log.Logging.Define<NetLog> { }
    public class NetworkException : Exception { }

    public class UdpListener
    {
        public IPAddress localAddress { get { return mConfig.netPeerConfig.LocalAddress; } }
        public int port { get { return mConfig.netPeerConfig.Port; } }
        public string appIdentifier { get { return mConfig.netPeerConfig.AppIdentifier; } }
        public MessageDispatcher dispatcher { get { return mDispatcher; } }
        public NetServer netServer { get { return mNetServer; } }

        public delegate void OnIncommingConnection(NetConnection connection);
        public delegate void OnConnectionStatusChanged(NetConnection connection, string reason);

        public class Configuration
        {
            public NetPeerConfiguration netPeerConfig;
            public OnIncommingConnection onIncommingConnection;
            public OnConnectionStatusChanged onConnectionStatusChanged;
        }

        public NetOutgoingMessage CreateMessage(MessageID id, FlatBufferBuilder fbb)
        {
            NetOutgoingMessage msg = mNetServer.CreateMessage();
            msg.Write((ushort)id);
            ushort len = (ushort)fbb.Offset;
            msg.Write(len);
            msg.Write(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset);
            return msg;
        }

        public NetSendResult SendMessage(
            NetOutgoingMessage msg,
            NetConnection recipient,
            NetDeliveryMethod method,
            int sequenceChannel = 0)
        {
            return mNetServer.SendMessage(msg, recipient, method, sequenceChannel);
        }

        public void SendMessage(
            NetOutgoingMessage msg,
            IList<NetConnection> recipients,
            NetDeliveryMethod method,
            int sequenceChannel = 0)
        {
            mNetServer.SendMessage(msg, recipients, method, sequenceChannel);
        }

        public void Start(Configuration config)
        {
            mConfig = config;
            mConfig.netPeerConfig.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
            #if VERBOSE_DEBUG
            mConfig.netPeerConfig.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            #endif

            mNetServer = new NetServer(mConfig.netPeerConfig);
            mNetServer.Start();

            NetLog.InfoFormat("{0} Network Initialized", appIdentifier);
        }

        public void Stop()
        {
            if (null != mNetServer)
            {
                mNetServer.Shutdown("Closed");
                mNetServer = null;
            }
        }

        public void Update()
        {
            if (null == mNetServer)
                return;

            NetIncomingMessage message;
            while (null != (message = mNetServer.ReadMessage()))
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
                mNetServer.Recycle(message);
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
            NetConnection conn = message.SenderConnection;
            mConfig.onIncommingConnection(conn);
            conn.Approve();
            NetLog.DebugFormat("{0} Approve connection {1}", appIdentifier, conn.RemoteEndPoint);
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
            ushort id = ushort.MaxValue;
            ByteBuffer byteBuffer = null;
            try
            {
                id = message.ReadUInt16();
                ushort len = message.ReadUInt16();
                byteBuffer = ByteBufferPool.Alloc(len);
                message.ReadBytes(byteBuffer.Data, 0, len);
                var result = dispatcher.Fire(message.SenderConnection, (MessageID)id, byteBuffer, message);
                if (result != MessageHandleResult.Processing)
                    ByteBufferPool.Dealloc(ref byteBuffer);
            }
            catch (Exception e)
            {
                ByteBufferPool.Dealloc(ref byteBuffer);
                NetLog.Exception("HandleData throws exception", e);

                if (id != ushort.MaxValue)
                    NetLog.Error("Caught exception when processing message " + (MessageID)id);
                else
                    NetLog.Error("Caught exception when processing message");
            }
        }

        NetServer mNetServer;
        Configuration mConfig;
        MessageDispatcher mDispatcher = new MessageDispatcher();
    }
}
