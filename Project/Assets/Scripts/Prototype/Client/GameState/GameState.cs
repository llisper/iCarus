using System;
using Lidgren.Network;

namespace Prototype.GameState
{
    public abstract class GameState
    {
        public static void Update(ref GameState gameState)
        {
            if (null != gameState.next)
            {
                GameState next = gameState.next;
                gameState.Destroy();
                next.Start();
                GameStateLog.InfoFormat(
                    "game state transition: {0} -> {1}",
                    gameState.GetType().Name,
                    next.GetType().Name);
                gameState = next;
            }
            gameState.Update();
        }

        public static void FixedUpdate(GameState gameState)
        {
            gameState.FixedUpdate();
        }

        GameState next;

        protected void TransitTo<T>(params object[] args) where T : GameState
        {
            GameState newState = (GameState)Activator.CreateInstance(typeof(T), args);
            next = newState;
        }

        public    abstract void Start();
        protected abstract void Update();
        protected virtual  void FixedUpdate() { }
        protected abstract void Destroy();

        protected Game.Game game { get { return Game.Game.Instance; } }

        protected bool ConnectionLost(NetConnectionStatus status)
        {
            return status == NetConnectionStatus.None || status > NetConnectionStatus.Connected;
        }
    }
}
