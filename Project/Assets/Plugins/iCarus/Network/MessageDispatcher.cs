using FlatBuffers;
using Protocol;

namespace iCarus.Network
{
    public delegate int MessageHandler(ByteBuffer byteBuffer);

    public class MessageDispatcher
    {
        public void Subscribe(MessageID id, MessageHandler handler)
        {
            mHandlers[(int)id] = handler;
        }

        public void Fire(MessageID id, ByteBuffer byteBuffer)
        {
            MessageHandler handler = mHandlers[(int)id];
            if (null != handler)
                handler(byteBuffer);
        }

        MessageHandler[] mHandlers = new MessageHandler[(int)MessageID.Count];
    }
}
