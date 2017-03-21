using UnityEngine;
using System.Collections;

namespace iCarus.Coex
{
    public class UseCoex : CoexBehaviour
    {

    }

    public class UseCoroutine : MonoBehaviour
    {

    }

    public class CompareWithUnityCoroutine : MonoBehaviour
    {
        #region inspector
        public bool useCoex;
        public int count;
        #endregion inspector

        UseCoex mUseCoex;
        UseCoroutine mUseCoroutine;

        void Awake()
        {
            mUseCoex = gameObject.AddComponent<UseCoex>();
            mUseCoroutine = gameObject.AddComponent<UseCoroutine>();
        }

        [ContextMenu("StartTest")]
        void StartTest()
        {
            if (useCoex)
            {
                for (int i = 0; i < count; ++i)
                    mUseCoex.StartCoroutine(Routine());
            }
            else
            {
                for (int i = 0; i < count; ++i)
                    mUseCoroutine.StartCoroutine(Routine());
            }
        }

        static IEnumerator Routine()
        {
            yield return CoexWaitForFixedUpdate.New();
            yield return null;
            yield return CoexWaitForEndOfFrame.New();
            while (true)
                yield return null;
        }
    }
}