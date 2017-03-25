using UnityEngine;
using Protocol;
using FlatBuffers;

namespace Prototype
{
    public class MovingSphere : MonoBehaviour, ITickObject
    {
        public int id { get { return 0; } }
        public TickObject type { get { return TickObject.MovingSphere; } }

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

        public int Snapshot(FlatBufferBuilder fbb, bool full)
        {
            if (full)
            {
                MovingSphereFull.StartMovingSphereFull(fbb);
                Vector3 vec3 = transform.position;
                MovingSphereFull.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                vec3 = transform.rotation.eulerAngles;
                MovingSphereFull.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                var offset = MovingSphereFull.EndMovingSphereFull(fbb);
                
                Protocol.MovingSphere.StartMovingSphere(fbb);
                Protocol.MovingSphere.AddFull(fbb, offset);
                return Protocol.MovingSphere.EndMovingSphere(fbb).Value;
            }
            else
            {
                MovingSphereDelta.StartMovingSphereDelta(fbb);
                Vector3 vec3 = transform.position;
                MovingSphereDelta.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                vec3 = transform.rotation.eulerAngles;
                MovingSphereDelta.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                var offset = MovingSphereDelta.EndMovingSphereDelta(fbb);

                Protocol.MovingSphere.StartMovingSphere(fbb);
                Protocol.MovingSphere.AddDelta(fbb, offset);
                return Protocol.MovingSphere.EndMovingSphere(fbb).Value;
            }
        }
    }
}
