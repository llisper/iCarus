using UnityEngine;
using System.Collections.Generic;

namespace PacMan
{
    public class BeanSpawnpoints : MonoBehaviour
    {
        public Transform ground;

        int mWidth;
        int mHeight;
        List<int> mUnused;
        List<int> mUsed;

        void Awake()
        {
            mWidth = (int)ground.localScale.x - 2;
            mHeight = (int)ground.localScale.z - 2;
            int count = mWidth * mHeight;
            mUnused = new List<int>(count);
            mUsed = new List<int>(count);
            for (int i = 0; i < count; ++i)
                mUnused.Add(i);
            Shuffle();
        }

        void Shuffle()
        {
            for (int i = 0; i < mUnused.Count; ++i)
                Swap(mUnused, i, UnityEngine.Random.Range(0, mUnused.Count));
        }

        void Swap(List<int> list, int i, int j)
        {
            int t = list[i];
            list[i] = list[j];
            list[j] = t;
        }

        public int Next(out Vector3 position)
        {
            position = Vector3.zero;
            if (mUnused.Count == 0)
                return -1;

            int index = mUnused[mUnused.Count - 1];
            position = new Vector3(index % mWidth, 0f, index / mWidth);
            mUsed.Add(index);
            mUnused.RemoveAt(mUnused.Count - 1);
            return index;
        }

        public void Release(int index)
        {
            int i = mUsed.IndexOf(index);
            if (i >= 0)
            {
                mUsed.RemoveAt(i);
                mUnused.Add(index);
                Swap(mUnused, mUnused.Count - 1, UnityEngine.Random.Range(0, mUnused.Count));
            }
        }
    }
}
