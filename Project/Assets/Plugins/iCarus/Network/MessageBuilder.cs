using System;
using FlatBuffers;

namespace iCarus.Network
{
    public static class MessageBuilder
    {
        public class Builder : IDisposable
        {
            FlatBufferBuilder mBuilder;
            bool mLock = false;

            internal Builder(FlatBufferBuilder fbb)
            {
                mBuilder = fbb;
                mBuilder.Clear();
            }

            internal Builder Lock()
            {
                if (mLock)
                    Exception.Throw<NetworkException>("FlatBufferBuilder has already been locked, recursive lock is not allowed");
                mLock = true;
                mBuilder.Clear();
                return this;
            }

            public FlatBufferBuilder fbb { get { return mBuilder; } }

            public static implicit operator FlatBufferBuilder(Builder builder)
            {
                return builder.mBuilder;
            }

            public void Dispose()
            {
                mLock = false;
                mBuilder.Clear();
            }
        }

        static Builder sBuilder;
        
        public static void Initialize(int defaultOutgoingMessageCapacity = 16384)
        {
            sBuilder = new Builder(new FlatBufferBuilder(defaultOutgoingMessageCapacity));
        }

        public static Builder Get()
        {
            return sBuilder.Lock();
        }
    }
}
