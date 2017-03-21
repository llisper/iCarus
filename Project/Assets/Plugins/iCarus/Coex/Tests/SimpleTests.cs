using UnityEngine;
using System;
using System.Collections;

namespace iCarus.Coex
{
    public class SimpleTests : CoexBehaviour
    {
        void Start()
        {
            StartCoroutine(Test());
        }

        IEnumerator Test()
        {
            yield return StartCoroutine(TestBasic());
            yield return StartCoroutine(TestExceptions());
            yield return StartCoroutine(TestUtilities());
            yield return StartCoroutine(ChainCoroutine_0());
        }

        #region basic
        IEnumerator Basic()
        {
            int i;
            for (i = 0; i < 10; ++i)
                yield return null;
            yield return i;
        }

        void TestDeprecatedApi()
        {
            // NOTE(llisper): i use Obsolete attribute to make these fuction calls causes compile errors
            /*
            ExpectException<CoroutineApiDeprecated>(() => StartCoroutine(""));
            ExpectException<CoroutineApiDeprecated>(() => StartCoroutine("", new object()));
            ExpectException<CoroutineApiDeprecated>(() => StartCoroutine_Auto(Basic()));
            Coroutine c = null;
            ExpectException<CoroutineApiDeprecated>(() => StopCoroutine(c));
            ExpectException<CoroutineApiDeprecated>(() => StopCoroutine(Basic()));
            ExpectException<CoroutineApiDeprecated>(() => StopCoroutine(""));
            */
        }

        IEnumerator TestBasic()
        {
            TestDeprecatedApi();
            Coex coex = StartCoroutine<int>(Basic());
            yield return coex;
            Assert(coex.state == Coex.State.Done);
            Assert(coex.yieldCount == 11);
            Assert(coex.ReturnValue<int>() == 10);
        }
        #endregion basic

        #region utilities
        IEnumerator TestUtilities()
        {
            for (int i = 0;  i < 3; ++i)
            {
                yield return null;
                Debug.Log("TestUtilities: yield null");
            }

            Debug.Log("time scale: " + Time.timeScale);
            for (int i = 0;  i < 3; ++i)
            {
                yield return CoexWaitForSeconds.New(1f);
                Debug.Log(string.Format("TestUtilities: {0} => yield WaitForSeconds", Time.unscaledTime));
            }

            Time.timeScale = 0.5f;
            Debug.Log("time scale: " + Time.timeScale);
            CoexWaitForSeconds wait = CoexWaitForSeconds.New(1f);
            for (int i = 0;  i < 3; ++i)
            {
                yield return wait;
                Debug.Log(string.Format("TestUtilities: {0} => yield WaitForSeconds", Time.unscaledTime));
            }

            Time.timeScale = 0f;
            Debug.Log("time scale: " + Time.timeScale);
            wait = CoexWaitForSeconds.New(1f, true);
            for (int i = 0;  i < 3; ++i)
            {
                yield return wait;
                Debug.Log(string.Format("TestUtilities: {0} => yield WaitForSeconds", Time.unscaledTime));
            }

            for (int i = 0;  i < 3; ++i)
            {
                yield return CoexWaitForEndOfFrame.New();
                Debug.Log("TestUtilities: yield WaitForEndOfFrame");
            }

            Time.timeScale = 1f;
            for (int i = 0;  i < 3; ++i)
            {
                yield return CoexWaitForFixedUpdate.New();
                Debug.Log("TestUtilities: yield WaitForFixedUpdate");
            }

            string url = string.Format("file:///{0}/Coex/Tests/WWW.html", Application.dataPath);
            WWW www = new WWW(url);
            yield return www;
            Assert(www.isDone);
        }

        IEnumerator ChainCoroutine_0()
        {
            Debug.Log("ChainCoroutine_0: before yield");
            yield return StartCoroutine(ChainCoroutine_1());
            Debug.Log("ChainCoroutine_0: after yield");
        }

        IEnumerator ChainCoroutine_1()
        {
            Debug.Log("ChainCoroutine_1: before yield");
            yield return StartCoroutine(ChainCoroutine_2());
            Debug.Log("ChainCoroutine_1: after yield");
        }

        IEnumerator ChainCoroutine_2()
        {
            Debug.Log("ChainCoroutine_2: before yield");
            yield return null;
            Debug.Log("ChainCoroutine_2: after yield");
        }
        #endregion utilities

        #region exception
        class TestException : Exception { }

        IEnumerator TestExceptions()
        {
            Coex c = StartCoroutine(ThrowsBeforeYield());
            yield return c;
            Assert(c.yieldCount == 0);
            Assert(c.state == Coex.State.Error);
            ExpectException<TestException>(c.CheckError);

            c = StartCoroutine(ThrowsAfterYield());
            yield return c;
            Assert(c.yieldCount == 1);
            Assert(c.state == Coex.State.Error);
            ExpectException<TestException>(c.CheckError);
        }

        IEnumerator ThrowsBeforeYield()
        {
            #pragma warning disable 162
            throw new TestException();
            yield return null;
            Debug.LogError("this line shall not be print out");
            #pragma warning restore 162
        }

        IEnumerator ThrowsAfterYield()
        { 
            #pragma warning disable 162
            yield return null;
            throw new TestException();
            yield return null;
            Debug.LogError("this line shall not be print out");
            #pragma warning restore 162
        }
        #endregion exception

        void Assert(bool condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }

        void ExpectException<T>(Action action) where T : Exception
        {
            try
            {
                action();
            }
            catch (T) 
            {
                return;
            }
            Assert(false);
        }
    }
}