using UnityEngine;
using System;

namespace Prototype.Game
{
    public class PlayerClient : IDisposable
    {
        public int id { get; private set; }
        public string playerName { get; private set; }
        public bool local { get { return id == Game.Instance.id; } }

        AvatarClient mAvatar;

        public static PlayerClient New(Protocol.Player data)
        {
            PlayerClient player = new PlayerClient()
            {
                id = data.Id,
                playerName = data.Name,
            };
            Transform parent = PlayerManagerClient.Instance.transform;
            player.mAvatar = AvatarClient.New(data.Id, player.local, parent);
            return player;
        }

        public void Dispose()
        {

        }
    }
}
