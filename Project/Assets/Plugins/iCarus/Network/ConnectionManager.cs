/*
using System.Collections.Generic;
using Lidgren.Network;
using IFGame.Utilities;

namespace IFGame.Battlecore
{
    internal class ConnectionManager
    {
        public void Add(Player player, NetConnection connection)
        {
            Remove(player);
            mPlayerConnections.Add(Tuple.Create(player, connection));
        }

        public void Remove(Player player)
        {
            for (int i = 0; i < mPlayerConnections.Count; ++i)
            {
                if (mPlayerConnections[i].item1 == player)
                {
                    mPlayerConnections.RemoveAt(i);
                    break;
                }
            }
        }

        public void Remove(NetConnection connection)
        {
            for (int i = 0; i < mPlayerConnections.Count; ++i)
            {
                if (mPlayerConnections[i].item2 == connection)
                {
                    mPlayerConnections.RemoveAt(i);
                    break;
                }
            }
        }

        public NetConnection GetConnection(Player player)
        {
            for (int i = 0; i < mPlayerConnections.Count; ++i)
            {
                if (mPlayerConnections[i].item1 == player)
                    return mPlayerConnections[i].item2;
            }
            return null;
        }

        public Player GetPlayer(NetConnection connection)
        {
            for (int i = 0; i < mPlayerConnections.Count; ++i)
            {
                if (mPlayerConnections[i].item2 == connection)
                    return mPlayerConnections[i].item1;
            }
            return null;
        }

        public void Clear()
        {
            mPlayerConnections.Clear();
        }

        List<Tuple<Player, NetConnection>> mPlayerConnections = new List<Tuple<Player, NetConnection>>();
    }
}
*/
