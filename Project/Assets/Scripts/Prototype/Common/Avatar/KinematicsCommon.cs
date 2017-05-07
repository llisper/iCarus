using UnityEngine;

namespace Prototype.Common
{
    public class KinematicsCommon : MonoBehaviour
    {
        public float speed = 10f;
        public float angularSpeed = 50f;

        CharacterController mController;

        void Awake()
        {
            mController = GetComponent<CharacterController>();
        }

        public void Warp(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
        }

        public void UpdateMotion(InputData inputData)
        {
            Vector3 dir = new Vector3(
                (int)((inputData.keyboard & 0x1)) - (int)((inputData.keyboard & 0x4) >> 2),
                0f,
                (int)((inputData.keyboard & 0x8) >> 3) - (int)((inputData.keyboard & 0x2) >> 1));
            dir = dir.normalized;

            Quaternion rot = transform.rotation;
            if (inputData.mouseHasHit)
            {
                Vector3 lookDir = inputData.mouseHit - transform.position;
                lookDir.y = 0f;
                lookDir.Normalize();
                rot = Quaternion.LookRotation(lookDir);
            }
            UpdateMotion(dir, rot);
        }

        public void UpdateMotion(Vector3 direction, Quaternion targetRotation)
        {
            if (direction != Vector3.zero)
                mController.Move(direction * speed * Time.deltaTime);
            transform.rotation = targetRotation;
        }
    }
}
