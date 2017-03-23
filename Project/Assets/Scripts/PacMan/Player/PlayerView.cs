using UnityEngine;
using System;

namespace PacMan
{
    public class PlayerView : MonoBehaviour
    {
        public float force;
        public float friction;
        [NonSerialized]
        public CharacterController controller;

        public Player player;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
        }
    }
}
