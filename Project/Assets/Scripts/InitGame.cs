using UnityEngine;
using UnityEngine.Rendering;

namespace BallGame
{
    class GameLog : iCarus.Log.Logging.Define<GameLog> { }

    class InitGame : MonoBehaviour
    {
        public bool dedicatedServer = false;
        bool isHeadless { get { return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null; } }

        void Awake()
        {
            dedicatedServer |= isHeadless;
            if (dedicatedServer)
            {
                new GameObject("DedicatedServer", typeof(DedicatedServer));
            }
            else
            {
                new GameObject("TestClient", typeof(TestClient));
            }
        }
    }
}
