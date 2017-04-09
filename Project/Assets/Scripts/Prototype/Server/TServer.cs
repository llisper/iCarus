﻿using UnityEngine;

using System.Collections.Generic;

using iCarus.Log;
using iCarus.Network;
using iCarus.Singleton;
using Protocol;
using Foundation;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public class TSLog : Logging.Define<TSLog> { }
    public sealed class TServer : SingletonBehaviour<TServer>
    {
        public float tickrate { get; private set; }
        public float updaterate { get; private set; }
        public uint tickCount { get { return mTickCount; } }
        public uint snapshotOverTick { get { return mSnapshotOverTick; } }
        public List<ITickObject> tickObjects { get { return mTickObjects; } }
        public NetServer netServer { get { return mUdpListener.netServer; } }

        public void StartServer()
        {
            tickrate = AppConfig.Instance.tickrate;
            updaterate = AppConfig.Instance.server.updaterate;
            mSnapshotOverTick = (uint)Mathf.FloorToInt(updaterate / tickrate);
            TSLog.InfoFormat("tickrate:{0}, updaterate:{1}, ss/t:{2}", tickrate, updaterate, mSnapshotOverTick);

            Time.fixedDeltaTime = tickrate;
            var prefab = Resources.Load("Prototype/Cube");
            GameObject go = (GameObject)Instantiate(prefab, transform);
            go.transform.position = new Vector3(3f, 0f, 0f);
            go.transform.rotation = Quaternion.identity;
            mTickObjects.Add(go.AddComponent<MovingSphere>());

            UdpListener.Configuration netConfig = new UdpListener.Configuration()
            {
                netPeerConfig = new NetPeerConfiguration(AppConfig.Instance.pacMan.appIdentifier)
                {
                    LocalAddress = System.Net.IPAddress.Any,
                    Port = AppConfig.Instance.pacMan.port,
                    MaximumConnections = AppConfig.Instance.pacMan.maxConnection,
                    DefaultOutgoingMessageCapacity = 1024,
                    SimulatedDuplicatesChance = AppConfig.Instance.simulatedDuplicatesChance,
                    SimulatedLoss = AppConfig.Instance.simulatedLoss,
                    SimulatedMinimumLatency = AppConfig.Instance.simulatedMinimumLatency,
                    SimulatedRandomLatency = AppConfig.Instance.simulatedRandomLatency,
                },
                onIncommingConnection = OnIncommingConnection,
                onConnectionStatusChanged = OnConnectionStatusChanged,
            };

            mUdpListener.Start(netConfig);
            mUdpListener.dispatcher.Subscribe(MessageID.InputDataArray, InputDataArrayHandler);
            mRunning = true;
            TSLog.Info("Start Running");
        }

        void Update()
        {
            if (mRunning)
                mUdpListener.Update();
        }

        void OnDestroy()
        {
            mUdpListener.Stop();
        }

        void FixedUpdate()
        {
            if (!mRunning) return;

            if (mTickCount > 0 && (mTickCount % mSnapshotOverTick) == 0)
            {
                foreach (var p in mPlayers)
                {
                    if (p.requestFullSnapshot)
                    {
                        if (p.connection.Status == NetConnectionStatus.Connected)
                        {
                            mPlayersToSendFull.Add(p);
                            p.requestFullSnapshot = false;
                        }
                    }
                    else
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

        void SendSnapshotToPlayers(List<TPlayer> players, bool full)
        {
            if (players.Count <= 0)
                return;

            var fbb = MessageBuilder.Lock();
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
                        int eventOffset = to.SnapshotEvent(fbb, mTickCount - mSnapshotOverTick + i);
                        if (eventOffset >= 0)
                        {
                            eventVector.offsets[eventVector.position++] = TickEventT.CreateTickEventT(
                                fbb,
                                mTickCount - mSnapshotOverTick + i,
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

            Snapshot.StartTickObjectVector(fbb, boxArray.position);
            var vecOffset = Helpers.SetVector(fbb, boxArray);
            OffsetArrayPool.Dealloc(ref boxArray);
            fbb.Finish(Snapshot.CreateSnapshot(
                fbb,
                mTickCount,
                mSnapshotOverTick,
                full,
                vecOffset).Value);

            foreach (var p in players)
            {
                NetOutgoingMessage msg = mUdpListener.CreateMessage(MessageID.Snapshot, fbb);
                if (!full)
                    p.AddAckInputs(msg);
                mUdpListener.SendMessage(
                    msg, 
                    p.connection,
                    NetDeliveryMethod.ReliableOrdered,
                    (int)SequenceChannel.Snapshot);
            }
            MessageBuilder.Unlock();
        }

        bool OnIncommingConnection(NetConnection connection, string name, out string denyReason)
        {
            denyReason = null;
            TSLog.InfoFormat("Incomming connection {0} - {1}", connection.RemoteEndPoint, name);
            TPlayer newPlayer = new TPlayer(name, connection);
            newPlayer.Init();
            mPlayers.Add(newPlayer);
            mTickObjects.Add(newPlayer);
            return true;
        }

        void OnConnectionStatusChanged(NetConnection connection, string reason)
        {
            if (connection.Status > NetConnectionStatus.Connected)
            {
                int index = mPlayers.FindIndex(p => p.connection == connection);
                if (-1 != index)
                {
                    mTickObjects.Remove(mPlayers[index]);
                    mPlayers[index].Destroy();
                    mPlayers.RemoveAt(index);
                }
            }
            TSLog.InfoFormat("Connection status changed {0} {1} {2}", connection.RemoteEndPoint, connection.Status, reason);
        }

        MessageHandleResult InputDataArrayHandler(NetConnection connection, ByteBuffer byteBuffer, NetIncomingMessage message)
        {
            int index = mPlayers.FindIndex(p => p.connection == connection);
            if (-1 != index)
            {
                InputDataArray ida = InstancePool.Get<InputDataArray>();
                InputDataArray.GetRootAsInputDataArray(byteBuffer, ida);
                mPlayers[index].UpdateInput(ida);
            }
            return MessageHandleResult.Finished;
        }

        uint mTickCount = 0;
        uint mSnapshotOverTick;
        bool mRunning = false;
        UdpListener mUdpListener = new UdpListener();
        List<TPlayer> mPlayers = new List<TPlayer>();
        List<TPlayer> mPlayersToSendDelta = new List<TPlayer>();
        List<TPlayer> mPlayersToSendFull = new List<TPlayer>();
        List<ITickObject> mTickObjects = new List<ITickObject>();
    }
}
