using Protocol;
using FlatBuffers;
using System.Collections.Generic;

namespace Prototype
{
    public interface ITickObject
    {
        int id { get; }
        TickObjectType type { get; }
        TickEventType eventType { get; }
        IList<ITickObject> children { get; }

        void Simulate();
        int  Snapshot(FlatBufferBuilder fbb, bool full);
        int  SnapshotEvent(FlatBufferBuilder fbb, uint tickCount);
    }

    public interface ITickObjectClient
    {
        int  id { get; }
        void FullUpdate(TickObject obj);
        void EventUpdate(TickEvent evt);
        void Lerping(float t, TickObject obj);

        bool predict { get; }
        void ApplyDeltaForPredict(TickObject obj);
        void Predict();

        IDictionary<int, ITickObjectClient> children { get; }
    }
}
