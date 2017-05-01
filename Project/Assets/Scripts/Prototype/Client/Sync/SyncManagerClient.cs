using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using Protocol;
using Foundation;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype.Game
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
        TickObjectDictionary mTickObjects = new TickObjectDictionary();
        ByteBuffer mProcessing = null;
        Queue<ByteBuffer> mCachedSnapshots = new Queue<ByteBuffer>();

        public void Initialize()
        {
            mInitialized = true;
        }

        public void Add(ITickObjectClient tickObject)
        {
            mTickObjects.Add(tickObject.id, tickObject);
        }

        public void Remove(ITickObjectClient tickObject)
        {
            mTickObjects.Remove(tickObject.id);
        }

        public void FullUpdate(Msg_SC_Snapshot snapshot)
        {
            mServerTick = snapshot.TickNow;
            mSimulateTicks = 0;
            mTickObjects.FullUpdate(new TickObjectDictionary.TickObjectEnumerator(snapshot));
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

                Msg_SC_Snapshot snapshot = InstancePool.Get<Msg_SC_Snapshot>();
                Msg_SC_Snapshot.GetRootAsMsg_SC_Snapshot(byteBuffer, snapshot);
                mTickObjects.ApplyDeltaForPredict(new TickObjectDictionary.TickObjectEnumerator(snapshot));
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
                mTickObjects.Simulate(nt, new TickObjectDictionary.TickObjectEnumerator(ss));
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
            mTickObjects.Predict();
        }
    }
}
