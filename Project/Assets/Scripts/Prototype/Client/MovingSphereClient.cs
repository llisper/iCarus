using UnityEngine;
using FlatBuffers;
using Protocol;

namespace Prototype
{
    public class MovingSphereClient : MonoBehaviour, ITickObjectClient
    {
        public int id { get { return 0; } }

        public void FullUpdate(uint tick, TickObjectBox box)
        {
            Protocol.MovingSphere sync = InstancePool.Get<Protocol.MovingSphere>();
            box.GetTickObject(sync);
            MovingSphereFull full = InstancePool.Get<MovingSphereFull>();
            sync.GetFull(full);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            full.GetPos(vec3);
            transform.position = new Vector3(vec3.X, vec3.Y, vec3.Z);
            full.GetRot(vec3);
            transform.rotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            mServerTick = tick;
            mPosition = transform.position;
            mRotation = transform.rotation;
        }

        public void Lerping(float t, uint tick, TickObjectBox box)
        {
            if (tick != mServerTick)
            {
                mServerTick = tick;
                mPosition = transform.position;
                mRotation = transform.rotation;
            }
                
            Protocol.MovingSphere sync = InstancePool.Get<Protocol.MovingSphere>();
            box.GetTickObject(sync);
            MovingSphereDelta delta = InstancePool.Get<MovingSphereDelta>();
            sync.GetDelta(delta);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            delta.GetPos(vec3);
            Vector3 targetPosition = new Vector3(vec3.X, vec3.Y, vec3.Z);
            delta.GetRot(vec3);
            Quaternion targetRotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            transform.position = Vector3.Lerp(mPosition, targetPosition, t);
            transform.rotation = Quaternion.Lerp(mRotation, targetRotation, t);
        }

        void Awake()
        {
            Renderer r = GetComponent<Renderer>();
            if (null != r)
                r.material.color = Color.red;
        }

        uint mServerTick;
        Vector3 mPosition;
        Quaternion mRotation;
    }
}
