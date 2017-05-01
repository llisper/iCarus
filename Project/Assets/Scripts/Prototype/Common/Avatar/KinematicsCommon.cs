using UnityEngine;

namespace Prototype.Common
{
    public class KinematicsCommon : MonoBehaviour
    {
        public float speed = 10f;
        public float angularSpeed = 50f;
        public bool interpolate = true;

        public Vector3 position { get { return mPos[now]; } }
        public Quaternion rotation { get { return mRot[now]; } }

        const int prv = 0;
        const int now = 1;

        CharacterController mController;
        Vector3[] mPos = new Vector3[2];
        Quaternion[] mRot = new Quaternion[2];
        float mLastUpdateTime = 0f;

        void Awake()
        {
            mController = GetComponent<CharacterController>();
            Warp();
        }

        void Update()
        {
            if (interpolate && mLastUpdateTime > 0f)
            {
                float nt = Mathf.Clamp01((Time.time - mLastUpdateTime) / Time.fixedDeltaTime);
                transform.position = Vector3.Lerp(mPos[prv], mPos[now], nt);
                transform.rotation = Quaternion.Lerp(mRot[prv], mRot[now], nt);
            }
        }

        public void Warp()
        {
            mPos[prv] = Vector3.zero;
            mPos[now] = transform.position;
            mRot[prv] = Quaternion.identity;
            mRot[now] = transform.rotation;
            mLastUpdateTime = 0f;
        }

        public void Warp(Vector3 pos, Quaternion rot)
        {
            transform.position = pos;
            transform.rotation = rot;
            Warp();
        }

        public void UpdateMotion(InputData inputData)
        {
            Vector3 dir = new Vector3(
                (int)((inputData.keyboard & 0x1)) - (int)((inputData.keyboard & 0x4) >> 2),
                0f,
                (int)((inputData.keyboard & 0x8) >> 3) - (int)((inputData.keyboard & 0x2) >> 1));
            dir = dir.normalized;

            Quaternion rot = rotation;
            if (inputData.mouseHasHit)
            {
                Vector3 lookDir = inputData.mouseHit - position;
                lookDir.y = 0f;
                lookDir.Normalize();
                rot = Quaternion.LookRotation(lookDir);
            }
            UpdateMotion(dir, rot);
        }

        public void UpdateMotion(Vector3 direction, Quaternion targetRotation)
        {
            if (interpolate)
            {
                mPos[prv] = mPos[now];
                transform.position = mPos[now];
                if (direction != Vector3.zero)
                    mController.Move(direction * speed * Time.deltaTime);
                mPos[now] = transform.position;
                transform.position = mPos[prv];

                mRot[prv] = mRot[now];
                transform.rotation = mRot[prv];
                mRot[now] = targetRotation;

                mLastUpdateTime = Time.fixedTime;
            }
            else
            {
                if (direction != Vector3.zero)
                    mController.Move(direction * speed * Time.deltaTime);
                transform.rotation = targetRotation;
            }
        }
    }
}
