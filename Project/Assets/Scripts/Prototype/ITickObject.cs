using FlatBuffers;
using Protocol;

namespace Prototype
{
    public interface ITickObject
    {
        int id { get; }
        TickObject type { get; }
        void SimulateFixedUpdate();
        int Snapshot(FlatBufferBuilder fbb, bool full);
    }

    public interface ITickObjectClient
    {
        int id { get; }
        void FullUpdate(uint tick, TickObjectBox box);
        void Lerping(float t, uint tick, TickObjectBox box);
    }
}
