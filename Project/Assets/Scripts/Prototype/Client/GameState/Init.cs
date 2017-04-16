using System;
using iCarus.Log;

namespace Prototype.GameState
{
    public class GameStateLog : Logging.Define<TCLog> { }
    public class Init : GameState
    {
        public override void Start()
        {
            Game.Instance.input.Init();
            GameStateLog.Info("Init Input");
            Game.Instance.syncManager.Init();
            GameStateLog.Info("Init SyncManager");
        }

        public override GameState Update()
        {
            return TransitTo<Connect>();
        }

        public override void Destroy() { }
    }
}
