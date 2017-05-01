using System.Collections.Generic;
using Protocol;
using FlatBuffers;

namespace Prototype.Server
{
    public class TickObjectList : List<ITickObject>
    {
        public void Simulate()
        {
            Simulate(this);
        }

        public VectorOffset SampleSnapshot(FlatBufferBuilder fbb, bool full)
        {
            return SampleSnapshot(this, fbb, full);
        }

        static void Simulate(IList<ITickObject> list)
        {
            if (null != list && list.Count > 0)
            {
                foreach (var obj in list)
                {
                    obj.Simulate();
                    Simulate(obj.children);
                }
            }
        }

        static uint snapshotOverTick { get { return SyncManager.Instance.snapshotOverTick; } }
        static uint tickCount { get { return SyncManager.Instance.tickCount; } }

        static VectorOffset SampleSnapshot(IList<ITickObject> list, FlatBufferBuilder fbb, bool full)
        {
            var tickObjectOffset = default(VectorOffset);
            if (null != list && list.Count > 0)
            {
                var boxArray = OffsetArrayPool.Alloc<TickObject>(list.Count);
                foreach (var obj in list)
                {
                    boxArray.offsets[boxArray.position++] = TickObject.CreateTickObject(
                        fbb,
                        obj.id,
                        obj.type,
                        SampleData(obj, fbb, full),
                        SampleEvents(obj, fbb, full),
                        SampleSnapshot(obj.children, fbb, full));
                }

                Msg_SC_Snapshot.StartTickObjectVector(fbb, boxArray.position);
                tickObjectOffset = Helpers.SetVector(fbb, boxArray);
                OffsetArrayPool.Dealloc(ref boxArray);
            }
            return tickObjectOffset;
        }

        static int SampleData(ITickObject obj, FlatBufferBuilder fbb, bool full)
        {
            return obj.Snapshot(fbb, full);
        }

        static VectorOffset SampleEvents(ITickObject obj, FlatBufferBuilder fbb, bool full)
        {
            VectorOffset vecOffset = default(VectorOffset);
            if (!full && obj.eventType != TickEventType.NONE)
            {
                var eventVector = OffsetArrayPool.Alloc<TickEvent>((int)snapshotOverTick);
                for (uint i = 0; i < snapshotOverTick; ++i)
                {
                    int eventOffset = obj.SnapshotEvent(fbb, tickCount - snapshotOverTick + i);
                    if (eventOffset > 0)
                    {
                        eventVector.offsets[eventVector.position++] = TickEvent.CreateTickEvent(
                            fbb,
                            tickCount - snapshotOverTick + i,
                            obj.eventType,
                            eventOffset);
                    }
                }

                TickObject.StartEventsVector(fbb, eventVector.position);
                vecOffset = Helpers.SetVector(fbb, eventVector);
                OffsetArrayPool.Dealloc(ref eventVector);
            }
            return vecOffset;
        }
    }
}
