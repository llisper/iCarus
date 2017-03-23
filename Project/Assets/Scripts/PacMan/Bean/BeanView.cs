using UnityEngine;
using System;

namespace PacMan
{
    public class BeanView : MonoBehaviour
    {
        public float rotateSpeed;
        [NonSerialized]
        public PhysicEvents physicEvents;

        void Awake()
        {
            physicEvents = GetComponent<PhysicEvents>();
        }

        void Update()
        {
            transform.rotation = Quaternion.Euler(0f, rotateSpeed * Time.time, 0f);
        }
    }
}
