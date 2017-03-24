using System;
using FlatBuffers;
using Protocol;

namespace PacMan
{
    public interface ITickObject
    {
        Protocol.TickObject type { get; }
        void SimulateFixedUpdate();
        int SnapshotDelta(FlatBufferBuilder fbb);
        int SnapshotFull(FlatBufferBuilder fbb);
    }


    public class TickObject2 : ITickObject
    {
        public TickObject type { get { return TickObject.TestObject2; } }

    }
}
