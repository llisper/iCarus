using UnityEngine;

using System;
using System.Collections;

using iCarus.Log;
using iCarus.Singleton;
using iCarus.Network;

using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace BallGame
{
    class DedicatedServer : MonoBehaviour
    {
        void Awake()
        {
            StartCoroutine(Init());
        }

        IEnumerator Init()
        {
            yield return StartCoroutine(Logging.Initialize("Config/server_log.xml"));
            // Singletons.Initialize();

            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            console.Initialize();
            console.SetTitle("DedicatedServer");
            Console.CancelKeyPress += (s, a) => 
            {
                Console.WriteLine("Catch Canel Key");
                a.Cancel = true;
            };
            #endif

            UdpServer.Configuration netConfig = new UdpServer.Configuration()
            {
                appIdentifier = "Test",
                onIncommingConnection = OnIncommingConnection,
                onConnectionStatusChanged = OnConnectionStatusChanged,
            };
            mServer.Start(netConfig);
            mServer.dispatcher.Subscribe(MessageID.HelloWorld, HandleHelloWorld);
            mInitialized = true;
        }

        void Update()
        {
            if (!mInitialized)
                return;

            try
            {
                mServer.Update();
            }
            catch (Exception e)
            {
                GameLog.Exception(e);
            }
        }

        void OnDestroy()
        {
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            console.Shutdown();
            #endif
        }

        int HandleHelloWorld(ByteBuffer byteBuffer)
        {
            HelloWorld msg = HelloWorld.GetRootAsHelloWorld(byteBuffer, InstancePool.Get<HelloWorld>());
            GameLog.Info("Hello from client: " + msg.Hail);
            return 0;
        }

        bool OnIncommingConnection(NetConnection connection, string name, out string denyReason)
        {
            denyReason = null;
            GameLog.InfoFormat("Incomming connection {0} - {1}", connection.RemoteEndPoint, name);
            return true;
        }

        void OnConnectionStatusChanged(NetConnection connection, string reason)
        {
            GameLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
        }

        bool mInitialized = false;
        UdpServer mServer = new UdpServer();
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        Windows.ConsoleWindow console = new Windows.ConsoleWindow();
        #endif
    }
}