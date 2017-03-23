using System;
using System.Collections;
using System.Collections.Generic;

using SimpleUI;
using Foundation;
using iCarus.Coex;
using iCarus.Singleton;

namespace PacMan
{
    public class PacMan : CoexBehaviour
    {
        public static PacMan Instance;
        public bool authorized { get; private set; }

        List<ISingleton> mSingletons = new List<ISingleton>();

        void Awake()
        {
            Instance = this;
        }

        void OnDestroy()
        {
            DestroySingletons();
            Instance = null;
        }

        void Start()
        {
            StartCoroutine(InitRoutine());
        }

        IEnumerator InitRoutine()
        {
            if (GameInitializer.Instance.isHeadless)
            {
                authorized = true;
                /*
                yield return StartCoroutine(InitSingletons(
                    typeof(SceneManager),
                    typeof(PlayerManager),
                    typeof(UdpServer)));

                if (mSingletons.Count == 0)
                    yield break;

                UdpServer.Instance.Listen();
                */
                yield break;
            }
            else
            {
                StartGame startGame = UI.Instance.Show<StartGame>();
                startGame.onCreateGame = () => StartCoroutine(CreatingGame());
                startGame.onJoinGame = (host, port) => StartCoroutine(JoiningGame(host, port));
            }
        }

        IEnumerator InitSingletons(params Type[] types)
        {
            foreach (Type type in types)
            {
                Coex coex = StartCoroutine<ISingleton>(Singletons.Add(type));
                yield return coex;

                try
                {
                    mSingletons.Add(coex.ReturnValue<ISingleton>());
                }
                catch (Exception e)
                {
                    DestroySingletons();
                    GameLog.Exception(e);
                }
            }
        }

        void DestroySingletons()
        {
            for (int i = mSingletons.Count - 1; i >= 0; --i)
                mSingletons[i].DestroySingleton();
            mSingletons.Clear();
        }

        IEnumerator CreatingGame()
        {
            authorized = true;
            yield return StartCoroutine(InitSingletons(
                typeof(NetId),
                typeof(SimulateManager),
                typeof(SceneManager),
                typeof(BeanManager),
                typeof(PlayerManager),
                typeof(Camera),
                typeof(InputSampler),
                typeof(UdpServer)));

            if (mSingletons.Count == 0)
                yield break;

            // TODO: re-think this
            //  1. init flatbuffer
            //  2. init sync-manager
            //  3. init scene
            //  4. init other managers
            //  5. (not dedicated)local player join the game
            //  6. start listening
            //  7. start the game(everything start running)

            SceneManager.Instance.Load(AppConfig.Instance.pacMan.map);
            BeanManager.Instance.Init();
            PlayerManager.Instance.AddLocalPlayer();
            UdpServer.Instance.Listen();
            SimulateManager.Instance.SimulateStart();
        }

        IEnumerator JoiningGame(string host, int port)
        {
            authorized = false;
            yield break;
            /*
            yield return StartCoroutine(InitSingletons(
                typeof(SceneManager),
                typeof(PlayerManager),
                typeof(UdpServer)));

            if (mSingletons.Count == 0)
                yield break;

            SceneManager.Instance.Load(AppConfig.Instance.pacMan.map);
            // TODO: create local player
            UdpServer.Instance.Listen();
            */
        }
    }
}
