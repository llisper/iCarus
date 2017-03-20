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

        public bool Compare(IFrameData o, out string detail)
        {
            detail = null;
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

        public bool Compare(IFrameData o, out string detail)
        {
            detail = null;
            var other = (RigidbodyData)o;
            bool equal = position == other.position &&
                         rotation == other.rotation &&
                         velocity == other.velocity &&
                         angularVelocity == other.angularVelocity;
            if (!equal)
            {
                detail = string.Format(
                    "p:{0},{1} r:{2},{3} v:{4},{5} av:{6},{7}",
                    position == other.position,
                    Vector3.Distance(position, other.position),
                    rotation == other.rotation,
                    Quaternion.Angle(rotation, other.rotation),
                    velocity == other.velocity,
                    Vector3.Distance(velocity, other.velocity),
                    angularVelocity == other.angularVelocity,
                    Vector3.Distance(angularVelocity, other.angularVelocity));

            }
            return equal;
        }

        public void Overwrite(Rigidbody r)
        {
            r.position = position;
            r.rotation = rotation;
            r.velocity = velocity;
            r.angularVelocity = angularVelocity;
        }
    }

    public class ProjectileData : IFrameData
    {
        public Vector3 position;
        public Vector3 velocity;

        public IFrameData Clone()
        {
            return new ProjectileData()
            {
                position = position,
                velocity = velocity,
            };
        }

        public bool Compare(IFrameData o, out string detail)
        {
            detail = null;
            var other = (ProjectileData)o;
            bool equal = position == other.position &&
                         velocity == other.velocity;
            if (!equal)
            {
                detail = string.Format(
                    "p:{0},{1} v:{2},{3}",
                    position == other.position,
                    Vector3.Distance(position, other.position),
                    velocity == other.velocity,
                    Vector3.Distance(velocity, other.velocity));
            }
            return equal;
        }
    }

    public class PlayerData : IFrameData
    {
        public Vector3 position;

        public IFrameData Clone()
        {
            return new PlayerData()
            {
                position = position,
            };
        }

        public bool Compare(IFrameData o, out string detail)
        {
            detail = null;
            var other = (PlayerData)o;
            bool equal = position == other.position;
            if (!equal)
            {
                detail = string.Format(
                    "p:{0},{1}",
                    position == other.position,
                    Vector3.Distance(position, other.position));
            }
            return equal;
        }
    }
}
