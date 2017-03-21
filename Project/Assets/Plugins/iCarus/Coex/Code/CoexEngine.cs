using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;

using iCarus.Singleton;

namespace iCarus.Coex
{
    /// <summary>
    /// coex协程引擎, 我尽量保证所有等待语句恢复执行时的在Unity协程中的调用时机
    /// </summary>
    public sealed class CoexEngine : SingletonBehaviour<CoexEngine>
    {
        #region global coroutine
        public new Coex StartCoroutine(IEnumerator routine)
        {
            return Instance.engineBehaviour.StartCoroutine(routine);
        }
        public Coex StartCoroutine<T>(IEnumerator routine)
        {
            return Instance.engineBehaviour.StartCoroutine<T>(routine);
        }
        public void StopCoroutine(Coex routine)
        {
            Instance.engineBehaviour.StopCoroutine(routine);
        }

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
        #endregion global coroutine

        #region internal
        void SingletonInit()
        {
            base.StartCoroutine(EndOfFrames());
        }

        internal class EngineBehaviour : CoexBehaviour { }

        internal EngineBehaviour engineBehaviour;
        internal List<CoexBehaviour> behaviours = new List<CoexBehaviour>(1024);

        internal void AddBehaviour(CoexBehaviour b)
        {
            if (!behaviours.Contains(b))
                behaviours.Add(b);
        }
        #endregion internal

        #region process
        public enum Process
        {
            None = -1,
            Update,
            FixedUpdate,
            EndOfFrames,
            Count,
        }

        void Loop(Process process)
        {
            for (int i = 0; i < behaviours.Count; )
            {
                CoexBehaviour b = behaviours[i];
                if (null == b)
                {
                    // NOTICE: this is a interesting part, b is not really null
                    // regarding to .NET, we're making use of unity's overload version
                    // of null condition test.
                    //
                    // and StopAllCoroutines must be called on b at this point, or else
                    // dangling Coex which is started by b will probably has a state
                    // other than Interrupt, could be Running or something else.
                    b.StopAllCoroutines();
                    b.Clear();
                    behaviours.RemoveAt(i);
                    continue;
                }

                if (!b.Loop(process))
                {
                    b.addToEngine = false;
                    behaviours.RemoveAt(i);
                    continue;
                }
                else
                {
                    ++i;
                }
            }
        }
        #endregion process

        #region unity message
        void Awake()
        {
            engineBehaviour = gameObject.AddComponent<EngineBehaviour>();
        }

        void Update()
        {
            Loop(Process.Update);
        }

        void FixedUpdate()
        {
            Loop(Process.FixedUpdate);
        }

        IEnumerator EndOfFrames()
        {
            UnityEngine.WaitForEndOfFrame wait = new UnityEngine.WaitForEndOfFrame();
            while (true)
            {
                yield return wait;
                Loop(Process.EndOfFrames);
            }
        }        
        #endregion unity message
    }
}