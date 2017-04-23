using UnityEngine;
using System.Collections.Generic;
using Prototype.Common;
using Lidgren.Network;
using FlatBuffers;

namespace Prototype
{
    public class Player
    {
        public enum State
        {
            Loading,
            FullSync,
            Playing,
            Linger,
        }

        public int id { get; private set; }
        public string playerName { get; private set; }
        public State state { get; set; }
        public NetConnection connection { get; private set; }
        public int choke { get { return Mathf.Max(0, mInputQueue.Count - SyncManager.Instance.inputchoke); } }
        public InputData currentInput { get { return mCurrentInput; } }

        uint[] mAckInputs;
        InputData mCurrentInput;
        Queue<InputData> mInputQueue = new Queue<InputData>();

        Player()
        {
            mAckInputs = new uint[SyncManager.Instance.snapshotOverTick];
        }

        public static Player New(int id, string playerName, NetConnection connection)
        {
            return new Player()
            {
                id = id,
                playerName = playerName,
                state = State.Loading,
                connection = connection,
            };
        }

        public void CFixedUpdate()
        {
            if (mInputQueue.Count <= 0)
            {
                mAckInputs[SyncManager.Instance.tickCount % mAckInputs.Length] = 0;
                mCurrentInput = default(InputData);
            }
            else
            {
                mCurrentInput = mInputQueue.Dequeue();
                mAckInputs[SyncManager.Instance.tickCount % mAckInputs.Length] = mCurrentInput.index;
            }
        }

        public void UpdateInput(Protocol.Msg_CS_InputDataArray inputDataArray)
        {
            Protocol.InputData inputData = InstancePool.Get<Protocol.InputData>();
            for (int i = 0; i < inputDataArray.InputDataLength; ++i)
            {
                inputDataArray.GetInputData(inputData, i);
                mInputQueue.Enqueue(new InputData()
                {
                    index = inputData.Index,
                    keyboard = inputData.Keyboard,
                    mouseHasHit = inputData.MouseHasHit,
                    mouseHit = new Vector3(inputData.MouseHit.X, inputData.MouseHit.Y, inputData.MouseHit.Z),
                });
            }
        }

        public void AddAckInputs(NetOutgoingMessage msg)
        {
            msg.Write(choke);
            for (uint i = 0; i < SyncManager.Instance.snapshotOverTick; ++i)
            {
                uint tickCount = SyncManager.Instance.tickCount - SyncManager.Instance.snapshotOverTick + i;
                uint index = mAckInputs[tickCount % mAckInputs.Length];
                msg.Write(index);
            }
        }
    }
}
