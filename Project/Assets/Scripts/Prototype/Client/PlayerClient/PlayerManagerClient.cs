using System.Collections.Generic;
using iCarus.Network;
using iCarus.Singleton;
using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype.Game
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

        // TODO:
        //  player update messages:
        //  1. player list (for newcomer)
        //  2. new player (for other players)
        //  3. remove player (for other players)
        public void UpdatePlayers(Msg_SC_UpdatePlayers msg)
        {
            if (msg.Clear)
            {
                foreach (var p in mPlayers)
                    p.Dispose();
                mPlayers.Clear();
            }

            Protocol.Player playerData = InstancePool.Get<Protocol.Player>();
            for (int i = 0; i < msg.AddPlayersLength; ++i)
            {
                msg.GetAddPlayers(playerData, i);
                PlayerClient newPlayer = PlayerClient.New(playerData);
                mPlayers.Add(newPlayer);
            }
           
            for (int i = 0; i < msg.RemovePlayersLength; ++i)
            {
                PlayerClient removePlayer = Get(msg.GetRemovePlayers(i));
                if (null != removePlayer)
                {
                    removePlayer.Dispose();
                    mPlayers.Remove(removePlayer);
                }
            }
        }

        MessageHandleResult UpdatePlayersHandler(
            NetConnection connection,
            ByteBuffer byteBuffer,
            NetIncomingMessage message)
        {
            Msg_SC_UpdatePlayers updateplayers = InstancePool.Get<Msg_SC_UpdatePlayers>();
            Msg_SC_UpdatePlayers.GetRootAsMsg_SC_UpdatePlayers(byteBuffer, updateplayers);
            UpdatePlayers(updateplayers);
            return MessageHandleResult.Finished;
        }

        List<PlayerClient> mPlayers = new List<PlayerClient>();
    }
}
