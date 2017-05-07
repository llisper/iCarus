using iCarus.Log;
using Prototype.Game;
using SimpleUI;

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

            SimpleUI.Login login = UI.Instance.Show<SimpleUI.Login>();
            login.onJoinGame = (host, port, name) =>
            {
                game.serverHost = host;
                game.serverPort = port;
                game.playerName = name;
                TransitTo<Connect>();
            };
        }

        protected override void Update() { }
        protected override void Destroy() { }
    }
}
