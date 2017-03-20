using System;
using UnityEngine;

namespace Experimental
{
    public abstract class FrameObject : MonoBehaviour, IFrame
    {
        [NonSerialized]
        public new Rigidbody rigidbody;
        public string identity { get { return name; } }

        protected virtual void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            Game.instance.SubscribeAll(this);
        }

        protected virtual void OnDestory()
        {
            Game.instance.UnsubscribeAll(this);
        }

        public abstract void Load(IFrameData frameData);
        public abstract IFrameData Save();
    }
}
