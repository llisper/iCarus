using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using iCarus.Network;
using FlatBuffers;
using Protocol;
using Foundation;
using Lidgren.Network;

namespace Prototype.Server
{
    public sealed class SyncManager : SingletonBehaviour<SyncManager>
    {
        public float tickrate { get; private set; }
        public float updaterate { get; private set; }
        public uint  snapshotOverTick { get { return mSnapshotOverTick; } }
        public int   inputchoke { get; private set; }
        public uint  tickCount { get { return mTickCount; } }

        public void Initialize()
        {
            tickrate = AppConfig.Instance.tickrate;
            updaterate = AppConfig.Instance.updaterate;
            inputchoke = Mathf.Max(1, Mathf.CeilToInt(AppConfig.Instance.cmdrate / tickrate));
            mSnapshotOverTick = (uint)Mathf.FloorToInt(updaterate / tickrate);
            Time.fixedDeltaTime = tickrate;
            TSLog.InfoFormat("tickrate:{0}, updaterate:{1}, ss/t:{2}", tickrate, updaterate, mSnapshotOverTick);
        }

        public void Add(ITickObject tickObject)
        {
            mTickObjects.Add(tickObject);
        }

        public void Remove(ITickObject tickObject)
        {
            mTickObjects.Remove(tickObject);
        }

        public void CFixedUpdate()
        {
            if (tickCount > 0 && (tickCount % mSnapshotOverTick) == 0)
            {
                foreach (var p in PlayerManager.Instance.players)
                {
                    if (p.state == Player.State.FullSync)
                    {
                        if (p.connection.Status == NetConnectionStatus.Connected)
                        {
                            mPlayersToSendFull.Add(p);
                            p.state = Player.State.Playing;
                        }
                    }
                    else if (p.state == Player.State.Playing)
                    {
                        mPlayersToSendDelta.Add(p);
                    }
                }

                SendSnapshotToPlayers(mPlayersToSendDelta, false);
                SendSnapshotToPlayers(mPlayersToSendFull, true);
                mPlayersToSendDelta.Clear();
                mPlayersToSendFull.Clear();
            }

            mTickObjects.Simulate();
            ++mTickCount;
        }

        public Offset<Msg_SC_Snapshot> SampleSnapshot(FlatBufferBuilder fbb, bool full)
        {
            return Msg_SC_Snapshot.CreateMsg_SC_Snapshot(
                fbb,
                tickCount,
                mSnapshotOverTick,
                full,
                mTickObjects.SampleSnapshot(fbb, full));
        }

        void SendSnapshotToPlayers(List<Player> players, bool full)
        {
            if (players.Count <= 0)
                return;

            using (var builder = MessageBuilder.Get())
            {
                FlatBufferBuilder fbb = builder.fbb;
                fbb.Finish(SampleSnapshot(fbb, full).Value);

                foreach (var p in players)
                {
                    NetOutgoingMessage msg = Server.Instance.netlayer.CreateMessage(MessageID.Msg_SC_Snapshot, fbb);
                    if (!full)
                        p.AddAckInputs(msg);
                    p.connection.SendMessage(msg, NetDeliveryMethod.ReliableOrdered, 0);
                }
            }
        }

        internal uint mTickCount = 0;
        internal uint mSnapshotOverTick;
        internal List<Player> mPlayersToSendDelta = new List<Player>();
        internal List<Player> mPlayersToSendFull = new List<Player>();
        internal TickObjectList mTickObjects = new TickObjectList();
    }
}
