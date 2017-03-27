using UnityEngine;

using System;
using System.Collections.Generic;

using Protocol;
using Foundation;
using FlatBuffers;

namespace Prototype
{
    public class SyncManager : MonoBehaviour
    {
        public bool hasFullUpdated { get { return mHasFullUpdated; } }
        public uint serverTick { get { return mServerTick; } }
        public float timer { get { return mTimer; } }
        public int snapshotCount { get { return mCachedSnapshots.Count; } }
        [NonSerialized]
        public float timeScale = 1f;
        [NonSerialized]
        public uint cacheBeforeLerping;

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
            mTimer = 0f;
            mHasFullUpdated = true;
        }

        public void AddDelta(uint tick, ByteBuffer byteBuffer)
        {
            if (!mHasFullUpdated || tick <= mServerTick)
                TCLog.WarnFormat("drop delta update, hasFullUpdated:{0}, tick:{1}, serverTick:{2}", mHasFullUpdated, tick, mServerTick);
            else
                mCachedSnapshots.Enqueue(byteBuffer);
        }

        public void AddTickObject(ITickObjectClient toc)
        {
            mTickObjects.Add(toc.id, toc);
        }

        void Update()
        {
            if (!mInitialized || 
                (null == mProcessing && mCachedSnapshots.Count < cacheBeforeLerping))
            {
                return;
            }

            if (null == mProcessing)
                mProcessing = mCachedSnapshots.Dequeue();

            timeScale = 1f;
            float sur = TClient.Instance.serverUpdaterate;
            if (mCachedSnapshots.Count > cacheBeforeLerping - 1)
            {
                int k = mCachedSnapshots.Count;
                timeScale = ((k + 1) * sur - mTimer) / (cacheBeforeLerping * sur - mTimer);
            }

            mTimer += Time.deltaTime * timeScale;
            Snapshot ss = InstancePool.Get<Snapshot>();

            while (true)
            {
                float nt = Mathf.Min(1f, mTimer / sur);
                Snapshot.GetRootAsSnapshot(mProcessing, ss);
                Lerping(nt, ss);

                if (mTimer >= sur)
                {
                    mTimer -= sur;
                    mServerTick = ss.TickNow;
                    ByteBufferPool.Dealloc(ref mProcessing);
                    if (mCachedSnapshots.Count > 0)
                    {
                        mProcessing = mCachedSnapshots.Dequeue();
                        if (mTimer > 0)
                            continue;
                    }
                }
                break;
            }
        }

        void Lerping(float t, Snapshot snapshot)
        {
            TickObjectBox tob = InstancePool.Get<TickObjectBox>();
            for (int i = 0; i < snapshot.TickObjectLength; ++i)
            {
                snapshot.GetTickObject(tob, i);
                ITickObjectClient tickObject;
                if (mTickObjects.TryGetValue(tob.Id, out tickObject))
                    tickObject.Lerping(t, mServerTick, tob);
            }
        }

        bool mInitialized = false;
        bool mHasFullUpdated = false;
        uint mServerTick = 0;
        float mTimer = 0f;
        Dictionary<int, ITickObjectClient> mTickObjects = new Dictionary<int, ITickObjectClient>();
        ByteBuffer mProcessing = null;
        Queue<ByteBuffer> mCachedSnapshots = new Queue<ByteBuffer>();
    }
}
