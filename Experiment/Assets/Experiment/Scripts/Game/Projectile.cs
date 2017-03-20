using UnityEngine;

namespace Experimental
{
    public class Projectile : FrameObject, ITrail
    {
        public float acceleration = 1f;

        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

        public override IFrameData Save()
        {
            return new ProjectileData()
            {
                position = rigidbody.position,
                velocity = mVelocity,
            };
        }

        public override void Load(IFrameData frameData)
        {
            ProjectileData data = (ProjectileData)frameData;
            transform.position = data.position;
            mVelocity = data.velocity;
        }

        void FixedUpdate()
        {
            var p = Game.instance.inputSampler.parameters;
            Vector3 dir = (new Vector3(p.horz, 0f, p.vert)).normalized;
            float t = Time.deltaTime;
            Vector3 at = dir * acceleration * t;
            Vector3 v0 = mVelocity;
            rigidbody.MovePosition(transform.position + (v0 + 0.5f * at) * t);
            mVelocity = v0 + at;
        }

        void OnCollisionEnter(Collision collision)
        {
            Vector3 dir = mVelocity.normalized;
            Vector3 nor = collision.contacts[0].normal;
            Vector3 reflect = (dir - Vector3.Dot(dir, nor) * 2f * nor).normalized;
            mVelocity = mVelocity.magnitude * reflect;

            Debug.LogFormat(
                "contact with {0}, {1} -> {2}, normal {3}, reflect {4}",
                collision.gameObject.name,
                transform.position,
                collision.contacts[0].point,
                collision.contacts[0].normal,
                reflect);
        }

        Vector3 mVelocity = Vector3.zero;
    }
}
