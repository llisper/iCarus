using UnityEngine;
using System.Collections.Generic;
using Protocol;
using FlatBuffers;

namespace Prototype
{
    public class SyncManager : MonoBehaviour
    {
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

        public void AddDelta(ByteBuffer byteBuffer)
        {
            if (!mHasFullUpdated)
                TCLog.Error("a full update must prior to delta update");
            else
                mCachedSnapshots.Enqueue(byteBuffer);
        }

        public void AddTickObject(ITickObjectClient toc)
        {
            mTickObjects.Add(toc.id, toc);
        }

        void Update()
        {
            if (null == mProcessing && mCachedSnapshots.Count < 2)
                return;

            if (null == mProcessing)
                mProcessing = mCachedSnapshots.Dequeue();

            float timeScale = 1f;
            float sur = TClient.Instance.serverUpdaterate;
            if (mCachedSnapshots.Count > 1)
            {
                int k = mCachedSnapshots.Count;
                timeScale = ((k + 1) * sur - mTimer) / (2 * sur - mTimer);
            }

            mTimer += Mathf.Min(Time.deltaTime * timeScale, sur);
            Snapshot ss = InstancePool.Get<Snapshot>();
            Snapshot.GetRootAsSnapshot(mProcessing, ss);
            Lerping(Mathf.Min(1f, mTimer / sur), ss);

            if (mTimer >= sur)
            {
                mServerTick = ss.TickNow;
                ByteBufferPool.Dealloc(ref mProcessing);
                if (mCachedSnapshots.Count == 0)
                {
                    mTimer = 0f;
                }
                else
                {
                    mProcessing = mCachedSnapshots.Dequeue();
                    mTimer -= sur;
                    Snapshot.GetRootAsSnapshot(mProcessing, ss);
                    Lerping(mTimer / sur, ss);
                }
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

        bool mHasFullUpdated = false;
        uint mServerTick = 0;
        float mTimer = 0f;
        Dictionary<int, ITickObjectClient> mTickObjects = new Dictionary<int, ITickObjectClient>();
        ByteBuffer mProcessing = null;
        Queue<ByteBuffer> mCachedSnapshots = new Queue<ByteBuffer>();
    }
}
