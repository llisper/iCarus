/*
using UnityEngine;
using Protocol;
using FlatBuffers;

namespace Prototype
{
    public class TPlayerClient : ITickObjectClient
    {
        public int id { get { return TClient.Instance.playerName.GetHashCode(); } }

        public void EventUpdate(TickEventT evt) { }

        public void FullUpdate(TickObjectBox box)
        {
            Protocol.Player data = InstancePool.Get<Protocol.Player>();
            box.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            transform.position = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            transform.rotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            mPosition = transform.position;
            mRotation = transform.rotation;
        }

        public void Lerping(float t, TickObjectBox box)
        {
            Protocol.Player data = InstancePool.Get<Protocol.Player>();
            box.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            Vector3 targetPosition = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            Quaternion targetRotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            transform.position = Vector3.Lerp(mPosition, targetPosition, t);
            transform.rotation = Quaternion.Lerp(mRotation, targetRotation, t);

            if (Mathf.Approximately(t, 1f))
            {
                mPosition = transform.position;
                mRotation = transform.rotation;
            }
        }

        public bool predict { get { return true; } }

        public void ApplyDelta(TickObjectBox box)
        {
            Protocol.Player data = InstancePool.Get<Protocol.Player>();
            box.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            Vector3 position = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            Quaternion rotation = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);

            mPosition = position;
            mRotation = rotation;
        }

        public void Predict()
        {
            var currentInput = TClient.Instance.input.current;
            if (currentInput.valid)
            {
                var inputQueue = TClient.Instance.input.inputQueue;
                Vector3 position = mPosition;
                Quaternion rotation = mRotation;
                for (int i = 0; i < inputQueue.Count; ++i)
                {
                    var inputData = inputQueue[i];

                    // NOTE: common code in server and client
                    Vector3 dir = new Vector3(
                        (int)((inputData.keyboard & 0x1)) - (int)((inputData.keyboard & 0x4) >> 2),
                        0f,
                        (int)((inputData.keyboard & 0x8) >> 3) - (int)((inputData.keyboard & 0x2) >> 1));
                    dir = dir.normalized;

                    if (dir != Vector3.zero)
                        position += dir * 10f * Time.deltaTime;

                    if (inputData.mouseHasHit)
                    {
                        Quaternion rot = Quaternion.LookRotation(inputData.mouseHit - mObject.transform.position);
                        rotation = Quaternion.Lerp(
                            rotation,
                            rot,
                            50f * TClient.Instance.tickrate);
                    }
                }

                if (inputQueue.Count == 0)
                    TCLog.Error("inputQueue empty");

                transform.position = position;
                transform.rotation = rotation;
            }
        }

        public void Init()
        {
            GameObject go = (GameObject)GameObject.Instantiate(Resources.Load("Prototype/Player"), TClient.Instance.transform);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            mObject = go;
        }

        Transform transform { get { return mObject.transform; } }

        GameObject mObject;
        Vector3 mPosition;
        Quaternion mRotation;
    }
}
*/
