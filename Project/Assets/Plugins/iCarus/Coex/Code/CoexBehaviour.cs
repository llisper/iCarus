using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace iCarus.Coex
{
    /// <summary>
    /// 需要使用Coex协程机制的脚本, 需要继承自CoexBehaviour类型
    /// MonoBehaviour中的一部分StartCoroutine和StopCoroutine接口被禁用
    /// </summary>
    public class CoexBehaviour : MonoBehaviour
    {
        #region coex api
        /// <summary>
        /// 启动一个协程, 无返回值
        /// </summary>
        /// <param name="routine">协程函数</param>
        /// <returns>Coex</returns>
        /// <exception>协程在第一个yield之前产生的异常将会在这次调用中直接抛出</exception>
        public new Coex StartCoroutine(IEnumerator routine)
        {
            return StartCoroutine(routine, null);
        }

        /// <summary>
        /// 启动一个协程, 执行返回值类型
        /// 当这个类型的值被yield出来, 协程将会立即停止执行
        /// 禁止作为返回值的类型是:
        /// 1. WWW
        /// 2. Coex
        /// 3. YieldInstruction
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="routine">协程函数</param>
        /// <returns>Coex</returns>
        /// <exception>协程在第一个yield之前产生的异常将会在这次调用中直接抛出</exception>
        /// <exception cref="CoroutineForbiddenReturnTypeException">使用了被禁止的类型作为返回值类型</exception>
        public Coex StartCoroutine<T>(IEnumerator routine)
        {
            return StartCoroutine(routine, typeof(T));
        }

        /// <summary>
        /// 启动多个协程, 顺序执行, 不支持返回值
        /// </summary>
        /// <param name="etors">协程函数列表</param>
        /// <returns></returns>
        public Coex BatchCoroutines(params IEnumerator[] etors)
        {
            if (etors.Length > 0)
                return StartCoroutine(_BatchCoroutines(etors));
            else
                return null;
        }

        IEnumerator _BatchCoroutines(IEnumerator[] etors)
        {
            foreach (var etor in etors)
            {
                Coex coex = StartCoroutine(etor);
                yield return coex;
                coex.CheckError();
            }
        }

        /// <summary>
        /// 停止协程的执行
        /// </summary>
        /// <param name="routine">协程object</param>
        public void StopCoroutine(Coex routine)
        {
            // NOTE(llisper): 
            // i don't remove coex from coexs at this point.
            // because this function might be called inside the coroutine itself, 
            // modify the coexs right away will corrupt the collection which is being iterating at this moment.
            // i leave all the removal works into Loop function
            if (routine.state == Coex.State.Running)
            {
                int i = (int)routine.process;
                if (i >= 0 && i < (int)CoexEngine.Process.Count)
                {
                    if (-1 != coexs[i].IndexOf(routine))
                        routine.Interrupt();
                }
            }
        }

        /// <summary>
        /// 停止所有协程的执行
        /// </summary>
        public new void StopAllCoroutines()
        {
            for (int i = 0; i < coexs.Length; ++i)
            {
                for (int j = 0; j < coexs[i].Count; ++j)
                    coexs[i][j].Interrupt();
            }
        }
        #endregion coex api

        #region disable original api
        [Obsolete("StartCoroutine(string) is obsoleted", true)]
        public new Coroutine StartCoroutine(string methodName) { return null; }

        [Obsolete("StartCoroutine(string, object) is obsoleted", true)]
        public new Coroutine StartCoroutine(string methodName, object value) { return null; }

        [Obsolete("StartCoroutine_Auto(IEnumerator) is obsoleted", true)]
        public new Coroutine StartCoroutine_Auto(IEnumerator routine) { return null; }

        [Obsolete("StopCoroutine(Coroutine) is obsoleted", true)]
        public new void StopCoroutine(Coroutine routine) { }

        [Obsolete("StopCoroutine(IEnumerator) is obsoleted", true)]
        public new void StopCoroutine(IEnumerator routine) { }

        [Obsolete("StopCoroutine(string) is obsoleted", true)]
        public new void StopCoroutine(string methodName) { }
        #endregion disable original api

        #region coex internal
        [NonSerialized]
        internal bool addToEngine;
        [NonSerialized]
        internal List<Coex>[] coexs = new List<Coex>[(int)CoexEngine.Process.Count];

        internal int coexCount
        {
            get
            {
                int cnt = 0;
                for (int i = 0; i < coexs.Length; ++i)
                    cnt += coexs[i].Count;
                return cnt;
            }
        }

        public CoexBehaviour()
        {
            for (int i = 0; i < coexs.Length; ++i)
                coexs[i] = new List<Coex>(8);
        }

        internal Coex StartCoroutine(IEnumerator routine, Type rtType)
        {
            if (!addToEngine)
            {
                CoexEngine.Instance.AddBehaviour(this);
                addToEngine = true;
            }

            CheckReturnType(rtType);

            Coex c = new Coex(routine, rtType);
            c.MoveNext();
            if (c.state == Coex.State.Running)
            {
                c.process = Dispatch(c);
                coexs[(int)c.process].Add(c);
            }
            return c;
        }

        internal void Clear()
        {
            addToEngine = false;
            for (int i = 0; i < coexs.Length; ++i)
            {
                for (int j = 0; j < coexs[i].Count; ++j)
                    coexs[i][j].Interrupt();
                coexs[i].Clear();
            }
        }

        internal void CheckReturnType(Type t)
        {
            if (t == typeof(WWW) || 
                t == typeof(Coex) || 
                t == typeof(YieldInstruction))
            {
                Exception.Throw<CoexException>("specify {0} as return value type is forbidden, it's been used by CoexEngine", t);
            }
        }

        internal bool Loop(CoexEngine.Process process)
        {
            if (!gameObject.activeInHierarchy || !enabled || coexCount == 0)
            {
                StopAllCoroutines();
                Clear();
                return false;
            }

            List<Coex> list = coexs[(int)process];
            for (int i = 0; i < list.Count; )
            {
                Coex coex = list[i];
                if (coex.state != Coex.State.Running)
                {
                    list.RemoveAt(i);
                    continue;
                }

                if (coex.frameCount != Time.frameCount)
                {
                    bool wait = false;
                    if (process == CoexEngine.Process.Update)
                    {
                        object rv = coex.returnValue;
                        if (null != rv)
                        {
                            wait = (rv is CoexWaitForSeconds && !((CoexWaitForSeconds)rv).timeout) ||
                                   (rv is WWW && !((WWW)rv).isDone) ||
                                   (rv is Coex && ((Coex)rv).state == Coex.State.Running);
                        }
                    }

                    if (!wait)
                    {
                        coex.MoveNext();
                        CoexEngine.Process dispatchProcess = Dispatch(coex);
                        if (dispatchProcess != process)
                        {
                            list.RemoveAt(i);
                            coexs[(int)dispatchProcess].Add(coex);
                            continue;
                        }
                    }
                }
                ++i;
            }
            return true;
        }

        CoexEngine.Process Dispatch(Coex coex)
        {
            object rv = coex.returnValue;
            if (null != rv)
            {
                if (rv is CoexWaitForFixedUpdate)
                    return CoexEngine.Process.FixedUpdate;
                else if (rv is CoexWaitForEndOfFrame)
                    return CoexEngine.Process.EndOfFrames;
            }
            return CoexEngine.Process.Update;
        }
        #endregion coex internal
    }
}