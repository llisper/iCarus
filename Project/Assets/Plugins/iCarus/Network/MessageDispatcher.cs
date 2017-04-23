using FlatBuffers;
using Lidgren.Network;
using Protocol;

namespace iCarus.Network
{
    public enum MessageHandleResult
    {
        Finished,
        Processing,
    }

    /// <summary>
    /// message handler
    /// </summary>
    /// <param name="connection">连接</param>
    /// <param name="byteBuffer">已读取的fb消息字节</param>
    /// <param name="message">读取fb消息字节之后的网络消息结构, 如果有附加数据, 可以继续在handler中读取</param>
    /// <returns></returns>
    public delegate MessageHandleResult MessageHandler(NetConnection connection, ByteBuffer byteBuffer, NetIncomingMessage message);

    public class MessageDispatcher
    {
        public void Subscribe(MessageID id, MessageHandler handler)
        {
            mHandlers[(int)id] += handler;
        }

        public void Unsubscribe(MessageID id, MessageHandler handler)
        {
            mHandlers[(int)id] -= handler;
        }

        public MessageHandleResult Fire(NetConnection connection, MessageID id, ByteBuffer byteBuffer, NetIncomingMessage message)
        {
            MessageHandler handler = mHandlers[(int)id];
            if (null != handler)
                return handler(connection, byteBuffer, message);
            return MessageHandleResult.Finished;
        }

        MessageHandler[] mHandlers = new MessageHandler[(int)MessageID.Count];
    }
}
