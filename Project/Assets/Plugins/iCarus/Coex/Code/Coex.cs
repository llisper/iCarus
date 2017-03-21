using UnityEngine;
using System;
using System.Collections;

namespace iCarus.Coex
{
    public class CoexException : Exception { }

    /// <summary>
    /// coroutine extension协程, 继承自CoexBehaviour的脚本调用StartCoroutine后返回的协程object
    /// </summary>
    public class Coex
    {
        /// <summary>
        /// 协程的执行状态
        /// </summary>
        public enum State
        {
            Running,
            Interrupted,
            Error,
            Done,
        }

        // 是否拥有返回值
        public bool hasReturnValue { get { return mReturnValueType != null; } }
        // 已经执行的yield次数
        public int yieldCount { get { return mYieldCount; } }
        // 执行状态
        public State state { get { return mState; } }

        /// <summary>
        /// 对执行异常检查, 如果没有异常, 取协程执行的返回值
        /// </summary>
        /// <typeparam name="T">返回值类型, 需要和调用StartCoroutine<T>传入的类型一致</typeparam>
        /// <returns>协程执行的返回值</returns>
        /// <exception>如果执行过程中产生了异常, 这次调用将会抛出那个异常</exception>
        /// <exception cref="CoroutineNoReturnValueException">如果协程没有返回值, 调用将会抛出异常</exception>
        public T ReturnValue<T>()
        {
            if (null == mReturnValueType)
                Exception.Throw<CoexException>("trying to access the return value of a coroutine which hasn't yield one");
            if (null != mException)
                throw mException;

            return (T)mReturnValue;
        }

        /// <summary>
        /// 检查协程执行的过程中是否有异常发生
        /// </summary>
        /// <exception>如果执行过程中产生了异常, 这次调用将会抛出那个异常</exception>
        public void CheckError()
        {
            if (null != mException)
                throw mException;
        }

        #region internal
        IEnumerator mRoutine;
        object mReturnValue;
        Type mReturnValueType;
        Exception mException;
        int mYieldCount;
        State mState;

        internal object returnValue { get { return mReturnValue; } }
        internal string routineName { get { return mRoutine.ToString(); } }
        internal Type returnValueType { get { return mReturnValueType; } }
        internal bool foldout;

        internal CoexEngine.Process process = CoexEngine.Process.None;
        internal int frameCount = 0;

        internal Coex(IEnumerator routine, Type returnValueType)
        {
            mRoutine = routine;
            mReturnValue = null;
            mReturnValueType = returnValueType;
            mException = null;
            mYieldCount = 0;
            mState = State.Running;
            frameCount = Time.frameCount;
        }

        internal void Interrupt()
        {
            if (mState == State.Running)
                mState = State.Interrupted;
        }

        internal void MoveNext()
        {
            try
            {
                MoveNextExceptional();
            }
            catch (Exception e)
            {
                mException = e;
                mState = State.Error;
            }
            frameCount = Time.frameCount;
        }

        internal void MoveNextExceptional()
        {
            if (!mRoutine.MoveNext())
            {
                mState = State.Done;
                return;
            }

            ++mYieldCount;
            mReturnValue = mRoutine.Current;
            if (null != mReturnValueType && 
                null != mReturnValue && 
                mReturnValue.GetType() == mReturnValueType)
            {
                mState = State.Done;
            }
            else if (null != mReturnValue)
            {
                if (mReturnValue is CoexWaitForSeconds)
                    ((CoexWaitForSeconds)mReturnValue).Reset();
            }
        }
        #endregion internal
    }
}