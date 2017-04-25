using System;

namespace Prototype
{
    public class PlayerClient : IDisposable
    {
        public int id { get; private set; }
        public string playerName { get; private set; }

        public static PlayerClient New(Protocol.Player data)
        {
            PlayerClient player = new PlayerClient()
            {
                id = data.Id,
                playerName = data.Name,
            };
            return player;
        }

        public void Dispose()
        {

        }
    }
}
