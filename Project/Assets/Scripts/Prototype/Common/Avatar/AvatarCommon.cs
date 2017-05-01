using UnityEngine;

namespace Prototype.Common
{
    public class AvatarCommon : MonoBehaviour
    {
        public KinematicsCommon kinematics { get; private set; }
        public Color color
        {
            get { return mMaterial.color; }
            set { mMaterial.color = value; }
        }

        Material mMaterial;

        void Awake()
        {
            mMaterial = GetComponent<Renderer>().material;
            kinematics = GetComponent<KinematicsCommon>();
        }
    }
}
