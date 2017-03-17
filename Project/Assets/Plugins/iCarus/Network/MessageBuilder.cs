using FlatBuffers;

namespace iCarus.Network
{
    public static class MessageBuilder
    {
        static FlatBufferBuilder sBuilder;
        static bool sLocked;
        
        internal static void Initialize(int defaultOutgoingMessageCapacity = 16384)
        {
            sBuilder = new FlatBufferBuilder(defaultOutgoingMessageCapacity);
        }

        public static FlatBufferBuilder Lock()
        {
            if (sLocked)
                Exception.Throw<NetworkException>("FlatBufferBuilder has already been locked, recursive lock is not allowed");
            return sBuilder;
        }

        public static void Unlock()
        {
            sLocked = false;
            sBuilder.Clear();
        }
    }
}
