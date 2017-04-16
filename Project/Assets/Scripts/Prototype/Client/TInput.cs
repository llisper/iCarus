using UnityEngine;
using System;
using System.Collections.Generic;
using Protocol;
using Foundation;
using FlatBuffers;
using iCarus.Network;
using Lidgren.Network;

namespace Prototype
{
    public class TInput 
    {
        public struct InputData
        {
            public uint index;          // sample index
            public byte keyboard;       // W-A-S-D
            public bool mouseHasHit;     
            public Vector3 mouseHit;    // X-Z position on Ground hit by mouse

            public bool valid { get { return index > 0; } }
        }

        public InputData current = default(InputData);
        public List<InputData> inputQueue { get { return mInputQueue; } }

        public void Init()
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
                var fbb = MessageBuilder.Lock();
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
                            Protocol.InputData.AddMouseHit(fbb, Vec3.CreateVec3(fbb, d.mouseHit.x, d.mouseHit.y, d.mouseHit.z));
                        array.offsets[array.position++] = Protocol.InputData.EndInputData(fbb);
                    }
                    InputDataArray.StartInputDataVector(fbb, array.position);
                    var offset = Helpers.SetVector(fbb, array);
                    fbb.Finish(InputDataArray.CreateInputDataArray(fbb, offset).Value);
                    connector.SendMessage(
                        connector.CreateMessage(MessageID.InputDataArray, fbb),
                        NetDeliveryMethod.UnreliableSequenced,
                        1);
                }
                catch (Exception e)
                {
                    TCLog.Exception(e);
                }
                finally
                {
                    MessageBuilder.Unlock();
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
