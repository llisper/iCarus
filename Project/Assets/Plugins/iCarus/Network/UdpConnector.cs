using System;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace iCarus.Network
{
    public class UdpConnector
    {
        public string host { get { return mConfig.host; } }
        public int port { get { return mConfig.port; } }
        public string appIdentifier { get { return mConfig.appIdentifier; } }
        public MessageDispatcher dispatcher { get { return mDispatcher; } }
        public NetConnectionStatus connectionStatus { get { return null != mClient ? mClient.ConnectionStatus : NetConnectionStatus.Disconnected; } }

        public delegate void OnNetStatusChanged(UdpConnector client, NetConnectionStatus status, string reason);

        public class Configuration
        {
            public string host = "localhost";
            public int port = 65534;
            public string appIdentifier = "UnknownApp";
            public int defaultOutgoingMessageCapacity = 16384;
            public OnNetStatusChanged onNetStatusChanged;
        }

        public void Start(Configuration config)
        {
            mConfig = config;

            NetPeerConfiguration netConfig = new NetPeerConfiguration(appIdentifier)
            {
                DefaultOutgoingMessageCapacity = mConfig.defaultOutgoingMessageCapacity,
            };

            #if VERBOSE_DEBUG
            netConfig.EnableMessageType(NetIncomingMessageType.VerboseDebugMessage);
            #endif

            mClient = new NetClient(netConfig);
            mClient.Start();
        }

        public void Connect(string name)
        {
            if (connectionStatus == NetConnectionStatus.Disconnected)
            {
                NetOutgoingMessage approval = mClient.CreateMessage();
                approval.Write(name);
                mClient.Connect(host, port, approval);
            }
        }

        public void Stop()
        {
            if (null != mClient)
                mClient.Shutdown("Disconnected");
        }

        public void Update()
        {
            if (null == mClient)
                return;

            NetIncomingMessage message;
            while (null != (message = mClient.ReadMessage()))
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
                                    "Unhandled message type:{0}, bytes:{1}",
                                    message.MessageType,
                                    message.LengthBytes);
                                break;
                            }
                    }
                }
                catch (Exception e)
                {
                    NetLog.Exception(e);
                }
                mClient.Recycle(message);
            }
        }

		public NetSendResult SendMessage(MessageID id, FlatBufferBuilder fbb, NetDeliveryMethod method, int sequenceChannel = 0)
        {
            if (null == mClient)
                Exception.Throw<NetworkException>("UdpClient.SendMessage is called before UdpClient.Start");
            if (fbb.Offset > ushort.MaxValue)
                throw new OverflowException(string.Format("fbb.Offset({0}) > ushort.MaxValue", fbb.Offset));

            NetOutgoingMessage msg = mClient.CreateMessage();
            msg.Write((ushort)id);
            ushort len = (ushort)fbb.Offset;
            msg.Write(len);
            msg.Write(fbb.DataBuffer.Data, fbb.DataBuffer.Position, fbb.Offset);
            return mClient.SendMessage(msg, method, sequenceChannel);
        }

        #region internal
        void HandleDebugMessage(NetIncomingMessage message)
        {
            switch (message.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                    {
                        NetLog.Debug(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.ErrorMessage:
                    {
                        NetLog.Error(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.WarningMessage:
                    {
                        NetLog.Warn(message.ReadString());
                        break;
                    }
                case NetIncomingMessageType.VerboseDebugMessage:
                    {
                        NetLog.Debug(message.ReadString());
                        break;
                    }
            }
        }

        void HandleStatusChanged(NetIncomingMessage message)
        {
            // <?> 接收到这个消息的时候, mClient.connectionStatus是否和status一致
            NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
            string reason = message.ReadString();
            if (null != mConfig.onNetStatusChanged)
                mConfig.onNetStatusChanged(this, status, reason);

            NetLog.DebugFormat(
                "connection({0}) status changed: {1}:{2}", 
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
                var result = dispatcher.Fire(message.SenderConnection, id, byteBuffer, message);
                if (result != MessageHandleResult.Processing)
                    ByteBufferPool.Dealloc(ref byteBuffer);
            }
            catch (Exception e)
            {
                ByteBufferPool.Dealloc(ref byteBuffer);
                NetLog.Exception("HandleData throws exception", e);
            }
        }

        NetClient mClient;
        Configuration mConfig;
        MessageDispatcher mDispatcher = new MessageDispatcher();
        #endregion internal
    }
}
