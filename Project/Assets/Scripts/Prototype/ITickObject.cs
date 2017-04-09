﻿using FlatBuffers;
using Protocol;

namespace Prototype
{
    public interface ITickObject
    {
        int id { get; }
        TickObject type { get; }
        TickEvent eventType { get; }
        void SimulateFixedUpdate();
        int Snapshot(FlatBufferBuilder fbb, bool full);
        int SnapshotEvent(FlatBufferBuilder fbb, uint tickCount);
    }

    public interface ITickObjectClient
    {
        int id { get; }
        void FullUpdate(uint tick, TickObjectBox box);
        void EventUpdate(TickEventT evt);
        void Lerping(float t, uint tick, TickObjectBox box);
    }
}
