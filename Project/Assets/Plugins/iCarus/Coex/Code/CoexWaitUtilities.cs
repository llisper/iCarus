using UnityEngine;

/// <summary>
/// Coex协程的等待类型, 直接使用Unity的协程等待类型将不会有任何效果(只能延迟一帧, 相当于yield return null)
/// </summary>
namespace iCarus.Coex
{
    /// <summary>
    /// 等待一定的时间
    /// </summary>
    public class CoexWaitForSeconds : YieldInstruction
    {
        /// <summary>
        /// 创建一个等待对象
        /// </summary>
        /// <param name="seconds">等待时间</param>
        /// <param name="ignoreTimescale">是否忽略时间尺度(可以在游戏暂停的时候仍然保持计时)</param>
        /// <returns>CoexWaitForSeconds</returns>
        public static CoexWaitForSeconds New(float seconds, bool ignoreTimescale = false)
        {
            return new CoexWaitForSeconds(seconds, ignoreTimescale);
        }

        CoexWaitForSeconds(float seconds, bool ignoreTimescale)
        { 
            mIgnoreTimescale = ignoreTimescale;
            mSeconds = seconds;
        }

        float mSeconds;
        float mTimeout;
        bool mIgnoreTimescale;

        internal bool timeout
        {
            get
            {
                return mIgnoreTimescale 
                    ? Time.unscaledTime >= mTimeout 
                    : Time.time >= mTimeout;
            }
        }

        internal void Reset()
        {
            mTimeout = mIgnoreTimescale ? Time.unscaledTime + mSeconds : Time.time + mSeconds;
        }
    }

    /// <summary>
    /// 等待直到当前帧结束
    /// </summary>
    public class CoexWaitForEndOfFrame : YieldInstruction
    {
        /// <summary>
        /// 创建一个等待对象
        /// </summary>
        /// <returns>CoexWaitForEndOfFrame</returns>
        public static CoexWaitForEndOfFrame New() { return sInstance; }

        CoexWaitForEndOfFrame() { }
        static CoexWaitForEndOfFrame sInstance = new CoexWaitForEndOfFrame();
    }

    /// <summary>
    /// 等待直到下一次的FixedUpdate调用
    /// </summary>
    public class CoexWaitForFixedUpdate : YieldInstruction
    {
        /// <summary>
        /// 创建一个等待对象
        /// </summary>
        /// <returns>CoexWaitForFixedUpdate</returns>
        public static CoexWaitForFixedUpdate New() { return sIntance; }

        CoexWaitForFixedUpdate() { }
        static CoexWaitForFixedUpdate sIntance = new CoexWaitForFixedUpdate();
    }
}