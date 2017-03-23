using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;

namespace PacMan
{
    public sealed class PlayerManager : SingletonBehaviour<PlayerManager>, ISimulate, INetSync
    {
        public int id { get { return NetId.PlayerManager; } }
        public Player localPlayer { get; private set; }

        UnityEngine.Object mPrefab;
        List<Player> mPlayers = new List<Player>();

        void SingletonInit()
        {
            mPrefab = Resources.Load("PacMan/Player");
            SimulateManager.Instance.Add(this);
        }

        public void SimulateFixedUpdate()
        {
        }

        public void AddLocalPlayer()
        {
            GameObject go = (GameObject)Instantiate(mPrefab, transform);
            go.transform.position = Vector3.zero;
            Player player = new Player(NetId.Instance.NextPlayer(), go.GetComponent<PlayerView>(), null);
            mPlayers.Add(player);
            SimulateManager.Instance.Add(player);
            localPlayer = player;
        }

        public void AddPlayer(Player player)
        {

        }

        public void RemovePlayer(Player player)
        {

        }
    }
}
