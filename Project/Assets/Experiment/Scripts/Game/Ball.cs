using UnityEngine;

namespace Experimental
{
    public class Ball : MonoBehaviour, IFrame, ITrail
    {
        public float force = 1f;

        Rigidbody mRigidbody;

        public string identity { get { return name; } }

        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

        void Awake()
        {
            mRigidbody = GetComponent<Rigidbody>();
            Game.instance.SubscribeAll(this);
        }

        void OnDestory()
        {
            Game.instance.UnsubscribeAll(this);
        }

        void FixedUpdate()
        {
            var p = Game.instance.inputSampler.parameters;
            Vector3 dir = (new Vector3(p.horz, 0f, p.vert)).normalized;
            mRigidbody.AddForce(dir * force, ForceMode.Force);
        }

        IFrameData IFrame.Save()
        {
            return new RigidbodyData(mRigidbody);
        }

        void IFrame.Load(IFrameData frameData)
        {
            ((RigidbodyData)frameData).Overwrite(mRigidbody);
        }
    }
}
