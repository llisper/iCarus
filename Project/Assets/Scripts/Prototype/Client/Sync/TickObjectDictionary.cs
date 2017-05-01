using System.Collections;
using System.Collections.Generic;
using Protocol;
using FlatBuffers;

namespace Prototype.Game
{
    public class TickObjectDictionary : Dictionary<int, ITickObjectClient>
    {
        public struct TickObjectEnumerator : IEnumerator<TickObject>
        {
            Msg_SC_Snapshot mSnapshot;
            TickObject mTickObject;
            int mIndex;
            int mLength;

            public TickObjectEnumerator(Msg_SC_Snapshot snapshot)
            {
                mSnapshot = snapshot;
                mTickObject = null;
                mIndex = -1;
                mLength = snapshot.TickObjectLength;
            }

            public TickObjectEnumerator(TickObject tickobject)
            {
                mSnapshot = null;
                mTickObject = tickobject;
                mIndex = -1;
                mLength = tickobject.TickObjectLength;
            }

            public TickObject Current
            {
                get
                {
                    TickObject instance = InstancePool.Get<TickObject>();
                    return null != mSnapshot 
                        ? mSnapshot.GetTickObject(instance, mIndex) 
                        : mTickObject.GetTickObject(instance, mIndex);
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    TickObject instance = InstancePool.Get<TickObject>();
                    return null != mSnapshot 
                        ? mSnapshot.GetTickObject(instance, mIndex) 
                        : mTickObject.GetTickObject(instance, mIndex);
                }
            }

            public void Dispose() { }

            public bool MoveNext()
            {
                if (mIndex < mLength)
                    ++mIndex;
                return mIndex < mLength;
            }

            public void Reset()
            {
                mIndex = -1;
            }
        }

        public void FullUpdate(TickObjectEnumerator etor)
        {
            FullUpdate(this, etor);
        }

        public void ApplyDeltaForPredict(TickObjectEnumerator etor)
        {
            ApplyDeltaForPredict(this, etor);
        }

        public void Simulate(float normalizedTime, TickObjectEnumerator etor)
        {
            Simulate(this, normalizedTime, etor);
        }

        public void Predict()
        {
            Predict(this);
        }

        #region internal
        static void FullUpdate(IDictionary<int, ITickObjectClient> dict, TickObjectEnumerator etor)
        {
            while (etor.MoveNext())
            {
                TickObject obj = etor.Current;
                ITickObjectClient tickobject;
                if (dict.TryGetValue(obj.Id, out tickobject))
                {
                    tickobject.FullUpdate(obj);
                    if (obj.TickObjectLength > 0 && null != tickobject.children)
                        FullUpdate(tickobject.children, new TickObjectEnumerator(obj));
                }
            }
        }

        static void ApplyDeltaForPredict(IDictionary<int, ITickObjectClient> dict, TickObjectEnumerator etor)
        {
            while (etor.MoveNext())
            {
                TickObject obj = etor.Current;
                ITickObjectClient tickobject;
                if (dict.TryGetValue(obj.Id, out tickobject) && tickobject.predict)
                    tickobject.ApplyDeltaForPredict(obj);
                if (obj.TickObjectLength > 0 && null != tickobject.children)
                    ApplyDeltaForPredict(tickobject.children, new TickObjectEnumerator(obj));
            }
        }
        
        static void Simulate(IDictionary<int, ITickObjectClient> dict, float normalizedTime, TickObjectEnumerator etor)
        {
            TickEvent ev = InstancePool.Get<TickEvent>();
            uint serverTick = SyncManagerClient.Instance.serverTick;
            while (etor.MoveNext())
            {
                TickObject obj = etor.Current;
                ITickObjectClient tickObject;
                if (dict.TryGetValue(obj.Id, out tickObject))
                {
                    for (int i = 0; i < obj.EventsLength; ++i)
                    {
                        obj.GetEvents(ev, i);
                        if (serverTick == ev.Tick)
                        {
                            tickObject.EventUpdate(ev);
                            break;
                        }
                    }

                    if (!tickObject.predict)
                        tickObject.Lerping(normalizedTime, obj);

                    if (obj.TickObjectLength > 0 && null != tickObject.children)
                        Simulate(tickObject.children, normalizedTime, new TickObjectEnumerator(obj));
                }
            }
        }

        static void Predict(IDictionary<int, ITickObjectClient> dict)
        {
            foreach (var kv in dict)
            {
                ITickObjectClient tickobject = kv.Value;
                if (tickobject.predict)
                    tickobject.Predict();
                if (null != tickobject.children && tickobject.children.Count > 0)
                    Predict(tickobject.children);
            }
        }
        #endregion internal
    }
}
