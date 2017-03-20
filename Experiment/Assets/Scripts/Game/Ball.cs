using UnityEngine;

namespace Experimental
{
    public class Ball : FrameObject, ITrail
    {
        public float force = 1f;

        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

        void FixedUpdate()
        {
            var p = Game.instance.inputSampler.parameters;
            Vector3 dir = (new Vector3(p.horz, 0f, p.vert)).normalized;
            rigidbody.AddForce(dir * force, ForceMode.Force);
        }

        public override IFrameData Save()
        {
            return new RigidbodyData(rigidbody);
        }

        public override void Load(IFrameData frameData)
        {
            ((RigidbodyData)frameData).Overwrite(rigidbody);
        }
    }
}
