using System;

namespace Prototype.GameState
{
    public abstract class GameState
    {

        public static GameState TransitTo<T>(GameState from, params object[] args) where T : GameState
        {
            if (null != from)
                from.Destroy();

            GameState newState = (GameState)Activator.CreateInstance(typeof(T), args);
            newState.Start();
            return newState;
        }

        protected GameState TransitTo<T>(params object[] args) where T : GameState
        {
            return TransitTo<T>(this, args);
        }

        public abstract void Start();
        public abstract GameState Update();
        public abstract void Destroy();
    }
}
