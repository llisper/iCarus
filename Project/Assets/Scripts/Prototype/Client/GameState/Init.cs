using System;
using iCarus.Log;

namespace Prototype.GameState
{
    public class GameStateLog : Logging.Define<TCLog> { }
    public class Init : GameState
    {
        public override void Start()
        {
            InputManager.Instance.Init();
            GameStateLog.Info("Init Input");
            SyncManagerClient.Instance.Init();
            GameStateLog.Info("Init SyncManager");
            TransitTo<Connect>();
        }

        protected override void Update() { }
        protected override void Destroy() { }
    }
}
