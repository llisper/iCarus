using UnityEngine;
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
        }

        public void Init()
        {
            float cmdrate = AppConfig.Instance.client.cmdrate;
            float tickrate = AppConfig.Instance.client.tickrate;
            mCmdOverTick = (uint)Mathf.Max(1, Mathf.CeilToInt(cmdrate / tickrate));
            mData = new InputData[mCmdOverTick];
            mIndex = 0;
        }

        public bool Current(out InputData inputData)
        {
            if (null == mData || mIndex == 0)
            {
                inputData = new InputData();
                return false;
            }
            inputData = mData[(mIndex - 1) % mCmdOverTick];
            return true;
        }

        public void UpdateInput(UdpConnector connector)
        {
            InputData inputData = new InputData();
            inputData.index = mIndex;
            inputData.keyboard = (byte)(GetKey(KeyCode.W) << 3 | GetKey(KeyCode.A) << 2 | GetKey(KeyCode.S) << 1 | GetKey(KeyCode.D));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (inputData.mouseHasHit = Physics.Raycast(ray, out hit, 100f, (1 << Layers.Ground)))
                inputData.mouseHit = hit.point;
            mData[mIndex % mData.Length] = inputData;
            mHistory.Enqueue(inputData);

            if (++mIndex % mData.Length == 0)
            {
                var fbb = MessageBuilder.Lock();
                var array = OffsetArrayPool.Alloc<Protocol.InputData>(mData.Length);
                for (int i = 0; i < mData.Length; ++i)
                {
                    InputData d = mData[i];
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
                    NetDeliveryMethod.UnreliableSequenced);

                /*
                var log = new System.Text.StringBuilder("send cmd [");
                for (int i = 0; i < mData.Length; ++i)
                    log.AppendFormat("{0},", mIndex - mData.Length + i);
                log[log.Length - 1] = ']';
                TCLog.Info(log);
                */
            }
        }

        public void AckInput()
        {

        }

        public void DrawGizmosSelected()
        {
            InputData current;
            if (Current(out current) && current.mouseHasHit)
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
        InputData[] mData;
        Queue<InputData> mHistory = new Queue<InputData>();
    }
}
