using System;

namespace iCarus
{
    /// <summary>
    /// GameException作为游戏的异常基类, 各个功能模块可以根据需求继承这个类型. 并做扩展
    /// 直接抛出System.Exception不是一个很好的选择, 因为处理异常的代码不能根据异常的类型来做不同的处理
    /// </summary>
    public class Exception : ApplicationException
    {
        /// <summary>
        /// 抛出类型为E的异常
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="reason"></param>
        public static void Throw<E>(string reason) where E : Exception, new()
        {
            throw new E() { mReason = reason };
        }

        /// <summary>
        /// 抛出类型为E的异常
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="format"></param>
        /// <param name="args"></param>
        public static void Throw<E>(string format, params object[] args) where E : Exception, new()
        {
            throw new E() { mReason = string.Format(format, args) };
        }

        /// <summary>
        /// 返回错误信息, 不可被子类override
        /// </summary>
        public sealed override string Message
        {
            get
            {
                string name = GetType().Name;
                name = name.EndsWith("Exception") ? name.Remove(name.Length - 9, 9) : name;
                return string.Format("<b>[{0}]: {1}</b> ", name, mReason);
            }
        }

        string mReason = string.Empty;
    }
}
