using iCarus;
using System.Collections.Generic;

namespace Prototype
{
    public class IdRange
    {
        public static readonly Tuple<int, int> players = new Tuple<int, int>(100, 200);
    }

    public class IdGen
    {
        public IdGen(Tuple<int, int> range)
        {
            mStart = range.item1;
            mLength = range.item2 - range.item1;
            mCurrent = 0;
        }

        public int Alloc()
        {
            for (int i = 0; i < mLength; ++i)
            {
                int id = mCurrent + mStart;
                mCurrent = (mCurrent + 1) % mLength;
                if (!mInUse.Contains(id))
                {
                    mInUse.Add(id);
                    return id;
                }
            }
            Exception.Throw<ServerException>("id[{0},{1}) just run out", mStart, mStart + mLength);
            return -1;
        }

        public void Release(int id)
        {
            mInUse.Remove(id);
        }

        int mStart;
        int mLength;
        int mCurrent;
        HashSet<int> mInUse = new HashSet<int>();
    }
}
