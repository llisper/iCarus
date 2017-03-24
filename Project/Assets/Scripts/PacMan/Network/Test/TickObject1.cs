using UnityEngine;
using System;
using Protocol;
using FlatBuffers;

namespace PacMan
{
    public class TickObject1 : MonoBehaviour, ITickObject
    {
        public TickObject type { get { return TickObject.TestObject1; } }

        public float speed = 1f;
        public float angularSpeed = 1f;

        float mRadian;
        float mDistToOrigin;

        void Awake()
        {
            mDistToOrigin = transform.position.magnitude;
        }

        public void SimulateFixedUpdate()
        {
            mRadian += Time.deltaTime * speed;
            transform.position = new Vector3(
                Mathf.Sin(mRadian) * mDistToOrigin,
                transform.position.y,
                Mathf.Cos(mRadian) * mDistToOrigin);

            transform.Rotate(Vector3.up, angularSpeed * Time.deltaTime * Mathf.Rad2Deg, Space.Self);
        }

        public int SnapshotDelta(FlatBufferBuilder fbb)
        {
            TestObject1.StartTestObject1(fbb);
            Vector3 pos = transform.position;
            TestObject1.AddPos(fbb, Vec3.CreateVec3(fbb, 1f, 2f, 3f));
            TestObject1.AddRot(fbb, Vec3.CreateVec3(fbb, 0f, 90f, 0f));
            return TestObject1.EndTestObject1(fbb).Value;
        }

        public int SnapshotFull(FlatBufferBuilder fbb)
        {
            throw new NotImplementedException();
        }
    }
}
