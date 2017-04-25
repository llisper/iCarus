using System.Collections.Generic;
using iCarus.Network;
using iCarus.Singleton;
using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public sealed class PlayerManagerClient : SingletonBehaviour<PlayerManagerClient>
    {
        public PlayerClient localplayer { get { return Get(Game.Instance.id); } }

        public void Initialize()
        {
            Game.Instance.netlayer.dispatcher.Subscribe(MessageID.Msg_SC_UpdatePlayers, UpdatePlayersHandler);
        }

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

        public void UpdatePlayers(Msg_SC_UpdatePlayers msg)
        {
            foreach (var p in mPlayers)
                p.Dispose();
            mPlayers.Clear();

            Protocol.Player playerData = InstancePool.Get<Protocol.Player>();
            for (int i = 0; i < msg.PlayersLength; ++i)
            {
                msg.GetPlayers(playerData, i);
                PlayerClient newPlayer = PlayerClient.New(playerData);
                mPlayers.Add(newPlayer);
            }
        }

        MessageHandleResult UpdatePlayersHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            return MessageHandleResult.Finished;
        }

        List<PlayerClient> mPlayers = new List<PlayerClient>();
    }
}
