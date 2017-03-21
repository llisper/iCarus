using UnityEngine;
using SimpleUI;

namespace PacMan
{
    public class PacMan : MonoBehaviour
    {
        void Start()
        {
            // TODO:
            //  0. init game select ui
            //  1. init scene manager(map)
            //  2. init bean manager(rand-seed,respawn-time,total)
            //  3. init player mananger
            //  4. start connection manager(max-connection)
            StartGame startGame = UI.Instance.Show<StartGame>();
            startGame.onCreateGame = OnCreateGame;
            startGame.onJoinGame = OnJoinGame;
        }

        void OnCreateGame()
        { }

        void OnJoinGame(string host, int port)
        { }
    }
}
