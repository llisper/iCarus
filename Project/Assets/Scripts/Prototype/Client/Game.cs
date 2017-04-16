using UnityEngine;
using iCarus.Network;
using iCarus.Singleton;
using Foundation;

namespace Prototype
{
    public sealed class Game : SingletonBehaviour<Game>
    {
        public float updaterate { get; private set; }
        public float tickrate { get; private set; }
        public float cmdrate { get; private set; }
        public uint snapshotOverTick { get; private set; }

        public UdpConnector netlayer { get; private set; }
        public TInput input { get; private set; }
        public SyncManager syncManager { get; private set; }
        public GameState.GameState gameState { get; private set; }

        public void Initialize()
        {
            updaterate = AppConfig.Instance.updaterate;
            tickrate = AppConfig.Instance.tickrate;
            cmdrate = AppConfig.Instance.cmdrate;
            snapshotOverTick = (uint)Mathf.FloorToInt(updaterate / tickrate);

            netlayer = new UdpConnector();
            input = new TInput();
            syncManager = new SyncManager();
            gameState = new GameState.Init();

            Time.fixedDeltaTime = tickrate;
            gameState.Start();
        }

        public void TransitGameStateTo<T>(params object[] args) where T : GameState.GameState
        {
            gameState = GameState.GameState.TransitTo<T>(gameState, args);
        }

        void Update()
        {
            var newState = gameState.Update();
            if (null != newState)
                gameState = newState;

            netlayer.Update();
        }

        void OnDestroy()
        {
            netlayer.Stop();
        }
    }
}
