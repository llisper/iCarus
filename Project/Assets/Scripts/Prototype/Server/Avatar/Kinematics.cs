using UnityEngine;
using System.Collections.Generic;
using Protocol;
using FlatBuffers;
using Prototype.Common;

namespace Prototype.Server
{
    public class Kinematics : Avatar.AvatarComponent
    {
        public override TickObjectType type { get { return TickObjectType.Kinematics; } }
        public override TickEventType eventType { get { return TickEventType.NONE; } }
        public override IList<ITickObject> children { get { return null; } }

        KinematicsCommon common { get { return avatar.common.kinematics; } }
        Transform transform { get { return common.transform; } } 

        public override void Simulate()
        {
            var inputData = avatar.inputProvider.current;
            if (!inputData.valid)
                return;

            common.UpdateMotion(inputData);
        }

        public override int Snapshot(FlatBufferBuilder fbb, bool full)
        {
            Protocol.Kinematics.StartKinematics(fbb);
            Vector3 vec3 = transform.position;
            Protocol.Kinematics.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
            vec3 = transform.rotation.eulerAngles;
            Protocol.Kinematics.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
            return Protocol.Kinematics.EndKinematics(fbb).Value;
        }

        public override int SnapshotEvent(FlatBufferBuilder fbb, uint tickCount)
        {
            return 0;
        }
    }
}
