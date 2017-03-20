using UnityEngine;

namespace Experimental
{
    public class Player : FrameObject, ITrail
    {
        public float speed = 1f;
        public bool _recordTrail = false;
        public bool recordTrail { get { return _recordTrail; } }

        public override void Load(IFrameData frameData)
        {
            transform.position = ((PlayerData)frameData).position;
        }

        public override IFrameData Save()
        {
            return new PlayerData() { position = transform.position };
        }

        protected override void Awake()
        {
            base.Awake();
            mController = GetComponent<CharacterController>();
        }

        void FixedUpdate()
        {
            var p = Game.instance.inputSampler.parameters;
            Vector3 dir = (new Vector3(p.horz, 0f, p.vert)).normalized;
            mController.Move(dir * speed * Time.deltaTime);
        }

        CharacterController mController;
    }
}
