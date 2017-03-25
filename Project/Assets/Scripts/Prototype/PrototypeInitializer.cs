using UnityEngine;
using System.Collections;
using SimpleUI;
using iCarus.Singleton;

namespace Prototype
{
    public class PrototypeInitializer : MonoBehaviour
    {
        void Awake()
        {
            StartCoroutine(AwakeRoutine());
        }

        IEnumerator AwakeRoutine()
        {
            yield return StartCoroutine(Singletons.Add<TServer>());
            yield return StartCoroutine(Singletons.Add<TClient>());

            if (GameInitializer.Instance.isHeadless)
            {
                TServer.Instance.StartServer();
            }
            else
            {
                StartPrototype ui = UI.Instance.Show<StartPrototype>();
                ui.onStartServer = TServer.Instance.StartServer;
                ui.onStartClient = TClient.Instance.StartClient;
            }
        }
    }
}
