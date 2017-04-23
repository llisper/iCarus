using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using iCarus.Network;
using FlatBuffers;
using Protocol;
using Foundation;
using Lidgren.Network;

namespace Prototype
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

            foreach (ITickObject to in mTickObjects)
                to.SimulateFixedUpdate();
            ++mTickCount;
        }

        void SendSnapshotToPlayers(List<Player> players, bool full)
        {
            if (players.Count <= 0)
                return;

            using (var builder = MessageBuilder.Get())
            {
                FlatBufferBuilder fbb = builder.fbb;
                var tickObjectOffset = default(VectorOffset);
                if (mTickObjects.Count > 0)
                {
                    var boxArray = OffsetArrayPool.Alloc<TickObjectBox>(mTickObjects.Count);
                    foreach (ITickObject to in mTickObjects)
                    {
                        int dataOffset = to.Snapshot(fbb, full);
                        VectorOffset eventVectorOffset = default(VectorOffset);
                        if (!full && to.eventType != TickEvent.NONE)
                        {
                            var eventVector = OffsetArrayPool.Alloc<TickEventT>((int)mSnapshotOverTick);
                            for (uint i = 0; i < mSnapshotOverTick; ++i)
                            {
                                int eventOffset = to.SnapshotEvent(fbb, tickCount - mSnapshotOverTick + i);
                                if (eventOffset >= 0)
                                {
                                    eventVector.offsets[eventVector.position++] = TickEventT.CreateTickEventT(
                                        fbb,
                                        tickCount - mSnapshotOverTick + i,
                                        to.eventType,
                                        eventOffset);
                                }
                            }

                            TickObjectBox.StartEventsVector(fbb, eventVector.position);
                            eventVectorOffset = Helpers.SetVector(fbb, eventVector);
                            OffsetArrayPool.Dealloc(ref eventVector);
                        }
                        boxArray.offsets[boxArray.position++] = TickObjectBox.CreateTickObjectBox(
                            fbb,
                            to.id,
                            to.type,
                            dataOffset,
                            eventVectorOffset);
                    }

                    Msg_SC_Snapshot.StartTickObjectVector(fbb, boxArray.position);
                    tickObjectOffset = Helpers.SetVector(fbb, boxArray);
                    OffsetArrayPool.Dealloc(ref boxArray);
                }
                fbb.Finish(Msg_SC_Snapshot.CreateMsg_SC_Snapshot(
                    fbb,
                    tickCount,
                    mSnapshotOverTick,
                    full,
                    tickObjectOffset).Value);

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
        internal List<ITickObject> mTickObjects = new List<ITickObject>();
    }
}
