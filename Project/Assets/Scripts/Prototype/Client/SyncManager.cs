using UnityEngine;

using System;
using System.Collections.Generic;

using Protocol;
using Foundation;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public class SyncManager : MonoBehaviour
    {
        public bool hasFullUpdated { get { return mHasFullUpdated; } }
        public uint serverTick { get { return mServerTick; } }
        public uint simulateTicks { get { return mSimulateTicks; } }
        public int snapshotCount { get { return mCachedSnapshots.Count; } }
        [NonSerialized]
        public float timeScale = 1f;
        [NonSerialized]
        public uint cacheBeforeLerping;

        bool mInitialized = false;
        bool mHasFullUpdated = false;
        uint mServerTick = 0;
        uint mSimulateTicks = 0;
        Dictionary<int, ITickObjectClient> mTickObjects = new Dictionary<int, ITickObjectClient>();
        ByteBuffer mProcessing = null;
        Queue<ByteBuffer> mCachedSnapshots = new Queue<ByteBuffer>();
        Queue<uint> mAckInputs = new Queue<uint>();

        public void Init()
        {
            cacheBeforeLerping = (uint)Mathf.CeilToInt(AppConfig.Instance.client.lerpdelay / AppConfig.Instance.server.updaterate);
            mInitialized = true;
        }

        public void FullUpdate(Snapshot snapshot)
        {
            TickObjectBox tob = InstancePool.Get<TickObjectBox>();
            for (int i = 0; i < snapshot.TickObjectLength; ++i)
            {
                snapshot.GetTickObject(tob, i);
                ITickObjectClient tickObject;
                if (mTickObjects.TryGetValue(tob.Id, out tickObject))
                    tickObject.FullUpdate(snapshot.TickNow, tob);
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
                for (int i = 0; i < TClient.Instance.snapshotOverTick; ++i)
                    mAckInputs.Enqueue(msg.ReadUInt32());
            }
        }

        public void AddTickObject(ITickObjectClient toc)
        {
            mTickObjects.Add(toc.id, toc);
        }

        public void SimulateFixedUpdate()
        {
            if (!mInitialized || 
                (null == mProcessing && mCachedSnapshots.Count < cacheBeforeLerping))
            {
                return;
            }

            if (null == mProcessing)
                mProcessing = mCachedSnapshots.Dequeue();

            timeScale = 1f;
            int simulateCount = 1;
            uint sot = TClient.Instance.snapshotOverTick;
            if (mCachedSnapshots.Count > cacheBeforeLerping - 1)
            {
                uint k = (uint)mCachedSnapshots.Count;
                uint ticksToSimulate = (k + 1) * sot - mSimulateTicks;
                uint ticksSupposeToSimulate = cacheBeforeLerping * sot - mSimulateTicks;
                simulateCount = Mathf.Max(1, Mathf.FloorToInt((float)ticksToSimulate / ticksSupposeToSimulate));
            }

            Snapshot ss = InstancePool.Get<Snapshot>();
            Snapshot.GetRootAsSnapshot(mProcessing, ss);
            for (int i = 0; i < simulateCount; ++i)
            {
                mServerTick = ss.TickNow - sot + mSimulateTicks;
                float nt = (float)(mSimulateTicks + 1) / sot;
                TClient.Instance.input.AckInput(mAckInputs.Dequeue());
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
                    Snapshot.GetRootAsSnapshot(mProcessing, ss);
                }
            }
        }

        void UpdateTickObjects(uint tickIndex, float nt, Snapshot snapshot)
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
                    tickObject.Lerping(nt, mServerTick, tob);
                }
            }
        }
    }
}
