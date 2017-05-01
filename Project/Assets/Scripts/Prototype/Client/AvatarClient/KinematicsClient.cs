using UnityEngine;
using System;
using System.Collections.Generic;
using Protocol;
using FlatBuffers;
using Prototype.Common;

namespace Prototype.Game
{
    public class KinematicsClient : AvatarClient.AvatarComponent
    {
        public override bool predict { get { return avatar.local; } }
        public override IDictionary<int, ITickObjectClient> children { get { return null; } }

        KinematicsCommon common { get { return avatar.common.kinematics; } }
        Transform transform { get { return common.transform; } }

        Vector3 mPosition;
        Quaternion mRotation;

        public override void Init(int id, AvatarClient avatar)
        {
            base.Init(id, avatar);
            common.interpolate = predict;
        }

        public override void FullUpdate(TickObject obj)
        {
            Parse(obj, out mPosition, out mRotation);
            transform.position = mPosition;
            transform.rotation = mRotation;
        }

        public override void EventUpdate(TickEvent evt) { }

        public override void Lerping(float t, TickObject obj)
        {
            Vector3 pos;
            Quaternion rot;
            Parse(obj, out pos, out rot);

            transform.position = Vector3.Lerp(mPosition, pos, t);
            transform.rotation = Quaternion.Lerp(mRotation, rot, t);

            if (Mathf.Approximately(t, 1f))
            {
                mPosition = transform.position;
                mRotation = transform.rotation;
            }
        }

        public override void ApplyDeltaForPredict(TickObject obj)
        {
            Parse(obj, out mPosition, out mRotation);
        }

        public override void Predict()
        {
            if (InputManager.Instance.current.valid)
            {
                var inputQueue = InputManager.Instance.inputQueue;
                common.Warp(mPosition, mRotation);
                foreach (var inputData in inputQueue)
                    common.UpdateMotion(inputData);
            }
        }

        void Parse(TickObject obj, out Vector3 pos, out Quaternion rot)
        {
            Protocol.Kinematics data = InstancePool.Get<Protocol.Kinematics>();
            obj.GetData(data);
            Vec3 vec3 = InstancePool.Get<Vec3>();
            data.GetPos(vec3);
            pos = new Vector3(vec3.X, vec3.Y, vec3.Z);
            data.GetRot(vec3);
            rot = Quaternion.Euler(vec3.X, vec3.Y, vec3.Z);
        }
    }
}
