using UnityEngine;

namespace Experimental
{
    public class InputParameters : IFrameData
    {
        public float horz;
        public float vert;

        public IFrameData Clone()
        {
            return new InputParameters()
            {
                horz = horz,
                vert = vert,
            };
        }

        public bool Compare(IFrameData o)
        {
            var other = (InputParameters)o;
            return Mathf.Approximately(horz, other.horz) &&
                   Mathf.Approximately(vert, other.vert);
        }

        public void Overwrite(InputParameters other)
        {
            other.horz = horz;
            other.vert = vert;
        }
    }

    public class RigidbodyData : IFrameData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
        public Vector3 angularVelocity;

        public RigidbodyData() { }

        public RigidbodyData(Rigidbody r)
        {
            // position = r.transform.position;
            // rotation = r.transform.rotation;
            position = r.position;
            rotation = r.rotation;
            velocity = r.velocity;
            angularVelocity = r.angularVelocity;
        }

        public IFrameData Clone()
        {
            return new RigidbodyData()
            {
                position = position,
                rotation = rotation,
                velocity = velocity,
                angularVelocity = angularVelocity,
            };
        }

        public bool Compare(IFrameData o)
        {
            var other = (RigidbodyData)o;
            return position == other.position &&
                   rotation == other.rotation &&
                   velocity == other.velocity &&
                   angularVelocity == other.angularVelocity;
        }

        public void Overwrite(Rigidbody r)
        {
            // r.transform.position = position;
            // r.transform.rotation = rotation;
            r.position = position;
            r.rotation = rotation;
            r.velocity = velocity;
            r.angularVelocity = angularVelocity;
        }
    }
}
