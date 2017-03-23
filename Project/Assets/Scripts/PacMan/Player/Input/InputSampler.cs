using UnityEngine;
using iCarus.Singleton;

namespace PacMan
{
    public sealed class InputSampler : SingletonBehaviour<InputSampler>, ISimulate, INetSync
    {
        public int id { get { return NetId.InputSampler; } }

        public float horz;
        public float vert;

        void SingletonInit()
        {
            SimulateManager.Instance.Add(this);
        }

        public void SimulateFixedUpdate()
        {
            horz = Input.GetAxis("Horizontal");
            vert = Input.GetAxis("Vertical");
        }
    }
}
