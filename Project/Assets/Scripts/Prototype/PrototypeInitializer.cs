using UnityEngine;
using SimpleUI;

namespace Prototype
{
    public class PrototypeInitializer : MonoBehaviour
    {
        void Awake()
        {
            if (GameInitializer.Instance.isHeadless)
            {
                Server.Server.Initialize();
            }
            else
            {
                StartPrototype ui = UI.Instance.Show<StartPrototype>();
                ui.onStartServer = Server.Server.Initialize;
                ui.onStartClient = Game.Game.Initialize;
            }
        }
    }
}
