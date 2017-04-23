
namespace Prototype
{
    public class PlayerClient
    {
        public int id { get; private set; }
        public string playerName { get; private set; }

        public static PlayerClient New(int id, string playerName)
        {
            PlayerClient player = new PlayerClient()
            {
                id = id,
                playerName = playerName,
            };
            return player;
        }
    }
}
