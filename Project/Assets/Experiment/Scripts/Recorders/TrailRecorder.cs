using System.Collections.Generic;
using UnityEngine;

namespace Experimental
{
    public class TrailRecorder : Recorder<ITrail>
    {
        public int startDrawFrame = 0;

        struct Data
        {
            public Vector3 position;
            public Quaternion rotation;
        }

        Dictionary<ITrail, List<Data>> mRecords = new Dictionary<ITrail, List<Data>>();

        void FixedUpdate()
        {
            if (recording)
            {
                foreach (var obj in objects)
                {
                    if (obj.recordTrail)
                    {
                        List<Data> list;
                        if (!mRecords.TryGetValue(obj, out list))
                            mRecords.Add(obj, list = new List<Data>());

                        MonoBehaviour x = (MonoBehaviour)obj;
                        list.Add(new Data()
                        {
                            position = x.transform.position,
                            rotation = x.transform.rotation,
                        });
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            foreach (var kv in mRecords)
            {
                for (int i = Mathf.Max(0, startDrawFrame) + 1; i < kv.Value.Count; ++i)
                {
                    Vector3 prev = kv.Value[i - 1].position;
                    Vector3 curr = kv.Value[i].position;
                    Gizmos.DrawLine(prev, curr);
                }
            }
        }
    }
}
