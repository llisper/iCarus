using UnityEngine;
using System;

namespace SimpleUI
{
    public class StartPrototype : MonoBehaviour
    {
        public Action onStartServer;
        public Action onStartClient;

        public void StartServer(GameObject _this)
        {
            if (null != onStartServer)
            {
                onStartServer();
                _this.SetActive(false);
            }
        }

        public void StartClient(GameObject _this)
        {
            if (null != onStartClient)
            {
                onStartClient();
                _this.SetActive(false);
            }
        }
    }
}
