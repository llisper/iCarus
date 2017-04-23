using UnityEngine;
using System.Collections;
using SimpleUI;
using iCarus.Singleton;

namespace Prototype
{
    public class PrototypeInitializer : MonoBehaviour
    {
        void Awake()
        {
            if (GameInitializer.Instance.isHeadless)
            {
                Server.Initialize();
            }
            else
            {
                StartPrototype ui = UI.Instance.Show<StartPrototype>();
                ui.onStartServer = Server.Initialize;
                ui.onStartClient = Game.Initialize;
            }
        }
    }
}
