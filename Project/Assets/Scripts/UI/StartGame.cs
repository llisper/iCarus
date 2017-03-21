using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
    public class StartGame : MonoBehaviour
    {
        public delegate void OnCreateGame();
        public OnCreateGame onCreateGame;

        public delegate void OnJoinGame(string host, int port);
        public OnJoinGame onJoinGame;

        public InputField hostInput;
        public InputField portInput;

        public void CreateGame()
        {
            if (null != onCreateGame)
                onCreateGame();
        }

        public void JoinGame()
        {
            if (null != onJoinGame)
                onJoinGame(hostInput.text, int.Parse(portInput.text));
        }
    }
}
