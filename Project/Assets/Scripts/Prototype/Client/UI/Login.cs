using UnityEngine;
using UnityEngine.UI;

namespace SimpleUI
{
    public class Login : MonoBehaviour
    {
        public delegate void OnJoinGame(string host, int port, string name);
        public OnJoinGame onJoinGame;

        public InputField hostInput;
        public InputField portInput;
        public InputField nameInput;

        public void JoinGame()
        {
            if (null != onJoinGame)
                onJoinGame(hostInput.text, int.Parse(portInput.text), nameInput.text);
            UI.Instance.Close(GetType());
        }
    }
}
