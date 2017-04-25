using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using Protocol;
using Foundation;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public sealed class SyncManagerClient : SingletonBehaviour<SyncManagerClient>
    {
        public bool hasFullUpdated { get { return mHasFullUpdated; } }
        public uint serverTick { get { return mServerTick; } }
        public uint simulateTicks { get { return mSimulateTicks; } }
        public int snapshotCount { get { return mCachedSnapshots.Count; } }
        public uint cacheSnapshots { get { return AppConfig.Instance.cachesnapshots; } }

        bool mInitialized = false;
        bool mHasFullUpdated = false;
        uint mServerTick = 0;
        uint mSimulateTicks = 0;
        Dictionary<int, ITickObjectClient> mTickObjects = new Dictionary<int, ITickObjectClient>();
        ByteBuffer mProcessing = null;
        Queue<ByteBuffer> mCachedSnapshots = new Queue<ByteBuffer>();

        public void Initialize()
        {
            mInitialized = true;
        }

        public void FullUpdate(Msg_SC_Snapshot snapshot)
        {
            TickObjectBox tob = InstancePool.Get<TickObjectBox>();
            for (int i = 0; i < snapshot.TickObjectLength; ++i)
            {
                snapshot.GetTickObject(tob, i);
                ITickObjectClient tickObject;
                if (mTickObjects.TryGetValue(tob.Id, out tickObject))
                    tickObject.FullUpdate(tob);
            }
            mServerTick = snapshot.TickNow;
            mSimulateTicks = 0;
            mHasFullUpdated = true;
            if (null != mProcessing)
                ByteBufferPool.Dealloc(ref mProcessing);
            while (mCachedSnapshots.Count > 0)
            {
                ByteBuffer bb = mCachedSnapshots.Dequeue();
                ByteBufferPool.Dealloc(ref bb);
            }
        }

        public void AddDelta(uint tick, ByteBuffer byteBuffer, NetIncomingMessage msg)
        {
            if (!mHasFullUpdated || tick <= mServerTick)
                TCLog.WarnFormat("drop delta update, hasFullUpdated:{0}, tick:{1}, current serverTick:{2}", mHasFullUpdated, tick, mServerTick);
            else
            {
                mCachedSnapshots.Enqueue(byteBuffer);
                int choke = msg.ReadInt32();
                InputManager.Instance.UpdateChoke(choke);
                for (int i = 0; i < Game.Instance.snapshotOverTick; ++i)
                    InputManager.Instance.AckInput(msg.ReadUInt32());
                ApplyDeltaToObjectThatPredicts(byteBuffer);
            }
        }

        public void AddTickObject(ITickObjectClient toc)
        {
            mTickObjects.Add(toc.id, toc);
        }

        public void SimulateFixedUpdate()
        {
            if (!mInitialized)
                return;

            Simulate();
            Predict();
        }

        void Simulate()
        {
            if (null == mProcessing && mCachedSnapshots.Count < cacheSnapshots)
                return;

            if (null == mProcessing)
                mProcessing = mCachedSnapshots.Dequeue();

            int simulateCount = 1;
            uint sot = Game.Instance.snapshotOverTick;
            if (mCachedSnapshots.Count > cacheSnapshots - 1)
            {
                uint k = (uint)mCachedSnapshots.Count;
                uint ticksToSimulate = (k + 1) * sot - mSimulateTicks;
                uint ticksSupposeToSimulate = cacheSnapshots * sot - mSimulateTicks;
                simulateCount = Mathf.Max(1, Mathf.FloorToInt((float)ticksToSimulate / ticksSupposeToSimulate));
            }

            Msg_SC_Snapshot ss = InstancePool.Get<Msg_SC_Snapshot>();
            Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(mProcessing, ss);
            for (int i = 0; i < simulateCount; ++i)
            {
                mServerTick = ss.TickNow - sot + mSimulateTicks;
                float nt = (float)(mSimulateTicks + 1) / sot;
                UpdateTickObjects(mSimulateTicks, nt, ss);
                ++mSimulateTicks;
                ++mServerTick;

                if (mSimulateTicks >= sot)
                {
                    mSimulateTicks = 0;
                    ByteBufferPool.Dealloc(ref mProcessing);
                    if (mCachedSnapshots.Count <= 0)
                        break;
                    mProcessing = mCachedSnapshots.Dequeue();
                    Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(mProcessing, ss);
                }
            }
        }

        void Predict()
        {
            foreach (var kv in mTickObjects)
            {
                if (kv.Value.predict)
                    kv.Value.Predict();
            }
        }

        void UpdateTickObjects(uint tickIndex, float nt, Msg_SC_Snapshot snapshot)
        {
            TickObjectBox tob = InstancePool.Get<TickObjectBox>();
            Protocol.TickEventT evt = InstancePool.Get<Protocol.TickEventT>();
            for (int i = 0; i < snapshot.TickObjectLength; ++i)
            {
                snapshot.GetTickObject(tob, i);
                ITickObjectClient tickObject;
                if (mTickObjects.TryGetValue(tob.Id, out tickObject))
                {
                    for (int j = 0; j < tob.EventsLength; ++j)
                    {
                        tob.GetEvents(evt, j);
                        if (mServerTick == evt.Tick)
                        {
                            tickObject.EventUpdate(evt);
                            break;
                        }
                    }

                    if (!tickObject.predict)
                        tickObject.Lerping(nt, tob);
                }
            }
        }

        void ApplyDeltaToObjectThatPredicts(ByteBuffer byteBuffer)
        {
            Msg_SC_Snapshot snapshot = InstancePool.Get<Msg_SC_Snapshot>();
            Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(byteBuffer, snapshot);

            TickObjectBox tob = InstancePool.Get<TickObjectBox>();
            for (int i = 0; i < snapshot.TickObjectLength; ++i)
            {
                snapshot.GetTickObject(tob, i);
                ITickObjectClient tickObject;
                if (mTickObjects.TryGetValue(tob.Id, out tickObject) && tickObject.predict)
                    tickObject.ApplyDelta(tob);
            }
        }
    }
}
