using UnityEngine;
using Foundation;
using Lidgren.Network;

namespace Prototype
{
    public class TInput : MonoBehaviour
    {
        public class Data
        {
            public uint index;      // sample index
            public byte keyboard;   // W-A-S-D
            public Vector2 mouseHit;// X-Z position on Ground hit by mouse

            public void Clear()
            {
                index = 0;
                keyboard = 0;
                mouseHit = Vector2.zero;
            }
        }

        public Data current
        {
            get
            {
                return null != mData && mIndex > 0 
                    ? mData[(mIndex - 1) % mCmdOverTick] 
                    : null;
            }
        }

        public void Init()
        {
            float cmdrate = AppConfig.Instance.client.cmdrate;
            float tickrate = AppConfig.Instance.client.tickrate;
            mCmdOverTick = (uint)Mathf.Max(1, Mathf.CeilToInt(cmdrate / tickrate));
            mData = new Data[mCmdOverTick];
            for (int i = 0; i < mData.Length; ++i)
                mData[i] = new Data();
            mIndex = 0;
        }

        public void UpdateInput(NetConnection connection)
        {
            float horz = Input.GetAxisRaw("Horizontal");
            float vert = Input.GetAxisRaw("Vertical");

        }

        void DrawGizmosSelected()
        {

        }

        uint mIndex;
        uint mCmdOverTick;
        Data[] mData;
    }
}
