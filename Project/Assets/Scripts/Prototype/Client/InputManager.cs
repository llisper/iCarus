using UnityEngine;
using System;
using System.Collections.Generic;
using Foundation;
using FlatBuffers;
using iCarus.Network;
using iCarus.Singleton;
using Lidgren.Network;
using Prototype.Common;

namespace Prototype.Game
{
    public sealed class InputManager : SingletonBehaviour<InputManager>
    {
        public InputData current = default(InputData);
        public List<InputData> inputQueue { get { return mInputQueue; } }

        public void Initialize()
        {
            float cmdrate = AppConfig.Instance.cmdrate;
            float tickrate = AppConfig.Instance.tickrate;
            mCmdOverTick = (uint)Mathf.Max(1, Mathf.CeilToInt(cmdrate / tickrate));
            mIndex = 1;
        }

        public void UpdateInput(UdpConnector connector)
        {
            current = default(InputData);
            if (mIndex % mCmdOverTick == 0)
            {
                if (mChoke > 0)
                {
                    --mChoke;
                    return;
                }
            }

            current.index = mIndex;
            current.keyboard = (byte)(GetKey(KeyCode.W) << 3 | GetKey(KeyCode.A) << 2 | GetKey(KeyCode.S) << 1 | GetKey(KeyCode.D));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (current.mouseHasHit = Physics.Raycast(ray, out hit, 100f, (1 << Layers.Ground)))
                current.mouseHit = hit.point;
            mInputQueue.Add(current);

            if ((mIndex++) % mCmdOverTick == 0)
            {

                using (var builder = MessageBuilder.Get())
                {
                    FlatBufferBuilder fbb = builder.fbb;
                    try
                    {
                        var array = OffsetArrayPool.Alloc<Protocol.InputData>((int)mCmdOverTick);
                        for (int i = 0; i < mCmdOverTick; ++i)
                        {
                            InputData d = mInputQueue[mInputQueue.Count - (int)mCmdOverTick + i];
                            Protocol.InputData.StartInputData(fbb);
                            Protocol.InputData.AddIndex(fbb, d.index);
                            Protocol.InputData.AddKeyboard(fbb, d.keyboard);
                            Protocol.InputData.AddMouseHasHit(fbb, d.mouseHasHit);
                            if (d.mouseHasHit)
                                Protocol.InputData.AddMouseHit(fbb, Protocol.Vec3.CreateVec3(fbb, d.mouseHit.x, d.mouseHit.y, d.mouseHit.z));
                            array.offsets[array.position++] = Protocol.InputData.EndInputData(fbb);
                        }
                        Protocol.Msg_CS_InputDataArray.StartInputDataVector(fbb, array.position);
                        var offset = Helpers.SetVector(fbb, array);
                        fbb.Finish(Protocol.Msg_CS_InputDataArray.CreateMsg_CS_InputDataArray(fbb, offset).Value);
                        connector.SendMessage(
                            connector.CreateMessage(Protocol.MessageID.Msg_CS_InputDataArray, fbb),
                            NetDeliveryMethod.UnreliableSequenced,
                            1);
                    }
                    catch (Exception e)
                    {
                        TCLog.Exception(e);
                    }
                }
            }
        }

        public void UpdateChoke(int choke)
        {
            mChoke = choke;
        }

        public void AckInput(uint ackIndex)
        {
            if (ackIndex > 0)
            {
                int i = 0;
                for (; i < mInputQueue.Count && mInputQueue[i].index <= ackIndex; ++i) ;
                mInputQueue.RemoveRange(0, i);
            }
        }

        public void DrawGizmosSelected()
        {
            if (current.valid && current.mouseHasHit)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(current.mouseHit, 0.25f);
            }
        }

        int GetKey(KeyCode keyCode)
        {
            return Input.GetKey(keyCode) ? 1 : 0;
        }

        uint mIndex;
        uint mCmdOverTick;
        int mChoke;
        List<InputData> mInputQueue = new List<InputData>();
    }
}
