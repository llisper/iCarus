using UnityEngine;
using iCarus;
using iCarus.Log;
using iCarus.Network;
using iCarus.Singleton;
using Foundation;

namespace Prototype
{
    public class TCLog : Logging.Define<TCLog> { }
    public class ClientException : Exception { }
    public sealed class Game : SingletonBehaviour<Game>
    {
        public float  updaterate { get; private set; }
        public float  tickrate { get; private set; }
        public float  cmdrate { get; private set; }
        public uint   snapshotOverTick { get; private set; }

        public int id { get; set; }
        public string playerName { get; private set; }

        public UdpConnector netlayer { get; private set; }
        public GameState.GameState gameState { get { return mGameState; } }

        GameState.GameState mGameState;

        public static void Initialize()
        {
            Game game = Singletons.Add<Game>();
            game.Run();
        }

        void Run()
        {
            updaterate = AppConfig.Instance.updaterate;
            tickrate = AppConfig.Instance.tickrate;
            cmdrate = AppConfig.Instance.cmdrate;
            snapshotOverTick = (uint)Mathf.FloorToInt(updaterate / tickrate);
            this.playerName = "anonymous player";

            Singletons.Add<InputManager>();
            Singletons.Add<SyncManagerClient>();
            Singletons.Add<PlayerManagerClient>();

            netlayer = new UdpConnector();
            mGameState = new GameState.Init();

            Time.fixedDeltaTime = tickrate;
            mGameState.Start();
        }

        void Update()
        {
            GameState.GameState.Update(ref mGameState);
        }

        void FixedUpdate()
        {
            netlayer.Update();
            GameState.GameState.FixedUpdate(mGameState);
        }

        void OnDestroy()
        {
            netlayer.Stop();
        }
    }
}
