using UnityEngine;
using System;

namespace PacMan
{
    public class PhysicEvents : MonoBehaviour
    {
        #region collision
        public Action<Collision> onCollisionEnter;
        public Action<Collision> onCollisionStay;
        public Action<Collision> onCollisionExit;

        void OnCollisionEnter(Collision collision)
        {
            if (null != onCollisionEnter)
                onCollisionEnter(collision);
        }

        void OnCollisionStay(Collision collision)
        {
            if (null != onCollisionStay)
                onCollisionStay(collision);
        }

        void OnCollisionExit(Collision collision)
        {
            if (null != onCollisionExit)
                onCollisionExit(collision);
        }
        #endregion collision

        #region trigger
        public Action<Collider> onTriggerEnter;
        public Action<Collider> onTriggerStay;
        public Action<Collider> onTriggerExit;

        void OnTriggerEnter(Collider other)
        {
            if (null != onTriggerEnter)
                onTriggerEnter(other);
        }

        void OnTriggerStay(Collider other)
        {
            if (null != onTriggerStay)
                onTriggerStay(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (null != onTriggerExit)
                onTriggerExit(other);
        }
        #endregion trigger
    }
}
