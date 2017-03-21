using UnityEngine;

using System.Collections;

using iCarus.Log;
using iCarus.Singleton;
using iCarus.Network;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace BallGame
{
    class TestClient : MonoBehaviour
    {
        void Awake()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return StartCoroutine(Logging.Initialize("Config/client_log.xml"));
            // Singletons.Initialize();

            UdpClient.Configuration netConfig = new UdpClient.Configuration()
            {
                appIdentifier = "Test",
                onNetStatusChanged = OnNetStatusChanged,
            };
            mClient.Start(netConfig);
            mClient.Connect("llisperzhang");
            mInitialized = true;
        }

        void Update()
        {
            if (!mInitialized)
                return;

            mClient.Update();
        }

        void OnNetStatusChanged(UdpClient client, NetConnectionStatus status, string reason)
        {
            GameLog.InfoFormat("Connection status changed {0} {1}", status, reason);
        }

        bool mInitialized = false;
        UdpClient mClient = new UdpClient();
    }
}
