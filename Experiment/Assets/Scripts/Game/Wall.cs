using UnityEngine;

namespace Experimental
{
    public class Wall : FrameObject, ITrail
    {
        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

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
