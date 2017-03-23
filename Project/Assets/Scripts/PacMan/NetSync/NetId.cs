using iCarus.Singleton;

namespace PacMan
{
    public sealed class NetId : Singleton<NetId>
    {
        public enum Type
        {
            Player, // [100-200)
            Bean,   // [200 - 1200)
            Count,
        }

        public const int BeanManager = 0;
        public const int PlayerManager = 1;
        public const int InputSampler = 2;

        int mPlayerIndex = 100; // [100, 200)
        int mBeanIndex = 200;   // [200, 1200)

        public int NextPlayer()
        {
            if (mPlayerIndex < 200)
                return mPlayerIndex++;
            iCarus.Exception.Throw<GameException>("player id overflow");
            return -1;
        }

        public int NextBean()
        {
            if (mBeanIndex < 1200)
                return mBeanIndex++;
            iCarus.Exception.Throw<GameException>("bean id overflow");
            return -1;
        }
    }
}
