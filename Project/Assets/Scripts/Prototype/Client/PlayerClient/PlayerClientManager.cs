using System.Collections.Generic;
using iCarus.Singleton;

namespace Prototype
{
    public sealed class PlayerClientManager : SingletonBehaviour<PlayerClientManager>
    {
        public PlayerClient localplayer { get { return Get(Game.Instance.id); } }

        public void Add(PlayerClient player)
        {
            if (null != Get(player.id))
                iCarus.Exception.Throw<ClientException>("player[{0}] already exists", player.id);
            mPlayers.Add(player);
        }

        public PlayerClient Get(int id)
        {
            foreach (var p in mPlayers)
            {
                if (p.id == id)
                    return p;
            }
            return null;
        }

        List<PlayerClient> mPlayers = new List<PlayerClient>();
    }
}
