using UnityEngine;

namespace Experimental
{
    public class Wall : MonoBehaviour, IFrame, ITrail
    {
        public string identity { get { return name; } }

        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

        Rigidbody mRigidbody;

        void Awake()
        {
            mRigidbody = GetComponent<Rigidbody>();
            Game.instance.SubscribeAll(this);
        }

        void OnDestory()
        {
            Game.instance.UnsubscribeAll(this);
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
