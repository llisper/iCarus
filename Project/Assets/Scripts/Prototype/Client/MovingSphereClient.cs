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
            Protocol.MovingSphere data = InstancePool.Get<Protocol.MovingSphere>();
            box.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            transform.position = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            transform.rotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            mPosition = transform.position;
            mRotation = transform.rotation;
        }

        public void EventUpdate(TickEventT evt)
        {
            Protocol.MovingSphereEvent ev = InstancePool.Get<Protocol.MovingSphereEvent>();
            evt.GetEv(ev);
            int colorValue = ev.Color;
            Color color = new Color(
                ((colorValue >> 24) & 0xff) / 255f,
                ((colorValue >> 16) & 0xff) / 255f,
                ((colorValue >> 8) & 0xff) / 255f,
                (colorValue & 0xff) / 255f);
            GetComponent<Renderer>().material.color = color;
        }

        public void Lerping(float t, uint tick, TickObjectBox box)
        {
            if (Mathf.Approximately(t, 1f))
            {
                mPosition = transform.position;
                mRotation = transform.rotation;
            }
                
            Protocol.MovingSphere data = InstancePool.Get<Protocol.MovingSphere>();
            box.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            Vector3 targetPosition = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            Quaternion targetRotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            transform.position = Vector3.Lerp(mPosition, targetPosition, t);
            transform.rotation = Quaternion.Lerp(mRotation, targetRotation, t);
        }

        Vector3 mPosition;
        Quaternion mRotation;
    }
}
