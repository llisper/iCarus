using UnityEngine;
using System;
using System.Collections.Generic;
using Protocol;
using FlatBuffers;
using Lidgren.Network;

namespace Prototype
{
    public class TPlayer : ITickObject
    {
        public string name;
        public NetConnection connection;
        public bool requestFullSnapshot = true;

        public TPlayer(string name, NetConnection connection)
        {
            this.name = name;
            this.id = name.GetHashCode();
            this.connection = connection;
        }

        public void Init()
        {
            GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("Prototype/Player"), TServer.Instance.transform);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            mObject = go;
        }

        public void Destroy()
        {
            GameObject.Destroy(mObject);
        }

        GameObject mObject;
        Queue<TInput.InputData> mInputDataQueue = new Queue<TInput.InputData>();

        public int id { get; private set; }
        public TickObject type { get { return TickObject.Player; } }

        public void SimulateFixedUpdate()
        {
            if (mInputDataQueue.Count <= 0)
                return;

            var inputData = mInputDataQueue.Dequeue();
            Vector3 dir = new Vector3(
                (int)((inputData.keyboard & 0x1)) - (int)((inputData.keyboard & 0x4) >> 2),
                0f,
                (int)((inputData.keyboard & 0x8) >> 3) - (int)((inputData.keyboard & 0x2) >> 1));
            dir = dir.normalized;

            if (dir != Vector3.zero)
                mObject.transform.position += dir * 10f * Time.deltaTime;

            if (inputData.mouseHasHit)
            {
                Quaternion rot = Quaternion.LookRotation(inputData.mouseHit - mObject.transform.position);
                mObject.transform.rotation = Quaternion.Lerp(
                    mObject.transform.rotation,
                    rot,
                    50f * Time.deltaTime);
            }

            // TSLog.InfoFormat("process input: frame:{0}, index:{1}", TServer.Instance.tickCount, inputData.index);
        }

        public int Snapshot(FlatBufferBuilder fbb, bool full)
        {
            Transform transform = mObject.transform;
            if (full)
            {
                PlayerFull.StartPlayerFull(fbb);
                Vector3 vec3 = transform.position;
                PlayerFull.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                vec3 = transform.rotation.eulerAngles;
                PlayerFull.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                var offset = PlayerFull.EndPlayerFull(fbb);
                
                Protocol.Player.StartPlayer(fbb);
                Protocol.Player.AddFull(fbb, offset);
                return Protocol.Player.EndPlayer(fbb).Value;
            }
            else
            {
                PlayerDelta.StartPlayerDelta(fbb);
                Vector3 vec3 = transform.position;
                PlayerDelta.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                vec3 = transform.rotation.eulerAngles;
                PlayerDelta.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
                var offset = PlayerDelta.EndPlayerDelta(fbb);

                Protocol.Player.StartPlayer(fbb);
                Protocol.Player.AddDelta(fbb, offset);
                return Protocol.Player.EndPlayer(fbb).Value;
            }
        }

        public void UpdateInput(InputDataArray inputDataArray)
        {
            Protocol.InputData inputData = InstancePool.Get<Protocol.InputData>();
            for (int i = 0; i < inputDataArray.InputDataLength; ++i)
            {
                inputDataArray.GetInputData(inputData, i);
                mInputDataQueue.Enqueue(new TInput.InputData()
                {
                    index = inputData.Index,
                    keyboard = inputData.Keyboard,
                    mouseHasHit = inputData.MouseHasHit,
                    mouseHit = new Vector3(inputData.MouseHit.X, inputData.MouseHit.Y, inputData.MouseHit.Z),
                });
            }
        }
    }
}
