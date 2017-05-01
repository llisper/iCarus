using iCarus.Log;
using Prototype.Game;

namespace Prototype.GameState
{
    public class GameStateLog : Logging.Define<TCLog> { }
    public class Init : GameState
    {
        public override void Start()
        {
            InputManager.Instance.Initialize();
            GameStateLog.Info("Init Input");
            SyncManagerClient.Instance.Initialize();
            GameStateLog.Info("Init SyncManagerClient");
            PlayerManagerClient.Instance.Initialize();
            GameStateLog.Info("Init PlayerManagerClient");
            TransitTo<Connect>();
        }

        protected override void Update() { }
        protected override void Destroy() { }
    }
}
