using FlatBuffers;
using Lidgren.Network;
using Protocol;

namespace iCarus.Network
{
    public delegate int MessageHandler(NetConnection connection, ByteBuffer byteBuffer);

    public class MessageDispatcher
    {
        public void Subscribe(MessageID id, MessageHandler handler)
        {
            mHandlers[(int)id] = handler;
        }

        public void Fire(NetConnection connection, MessageID id, ByteBuffer byteBuffer)
        {
            MessageHandler handler = mHandlers[(int)id];
            if (null != handler)
                handler(connection, byteBuffer);
        }

        MessageHandler[] mHandlers = new MessageHandler[(int)MessageID.Count];
    }
}
