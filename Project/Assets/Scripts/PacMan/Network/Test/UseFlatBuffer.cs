using UnityEngine;
using System;
using Protocol;
using FlatBuffers;
using iCarus.Network;

namespace PacMan
{
    class UseFlatBuffer : MonoBehaviour
    {
        void Start()
        {
            FlatBuffersInitializer.Initialize(typeof(ProtocolInitializer).Assembly);
            MessageBuilder.Initialize();

            var fbb = MessageBuilder.Lock();

            var BoxArray = OffsetArrayPool.Alloc<TickObjectBox>(2);

            TestObject1.StartTestObject1(fbb);
            TestObject1.AddPos(fbb, Vec3.CreateVec3(fbb, 1f, 2f, 3f));
            TestObject1.AddRot(fbb, Vec3.CreateVec3(fbb, 0f, 90f, 0f));
            var to1Offset = TestObject1.EndTestObject1(fbb);

            BoxArray.offsets[BoxArray.position++] = TickObjectBox.CreateTickObjectBox(
                fbb,
                TickObject.TestObject1,
                to1Offset.Value);

            TestObject2.StartTestObject2(fbb);
            TestObject2.AddCount(fbb, 7);
            var to2Offset = TestObject2.EndTestObject2(fbb);

            BoxArray.offsets[BoxArray.position++] = TickObjectBox.CreateTickObjectBox(
                fbb,
                TickObject.TestObject2,
                to2Offset.Value);

            SnapshotDelta.StartTickObjectVector(fbb, BoxArray.position);
            var vecOffset = Helpers.SetVector(fbb, BoxArray);

            fbb.Finish(SnapshotDelta.CreateSnapshotDelta(fbb, vecOffset).Value);

            int len = fbb.DataBuffer.Length - fbb.DataBuffer.Position;
            Debug.LogFormat("serialize -> size:{0}, length:{1}", len, fbb.DataBuffer.Data.Length);

            ByteBuffer byteBuffer = ByteBufferPool.Alloc(len);
            Array.Copy(fbb.DataBuffer.Data, fbb.DataBuffer.Position, byteBuffer.Data, 0, len);
            MessageBuilder.Unlock();

            SnapshotDelta ss = InstancePool.Get<SnapshotDelta>();
            TickObjectBox Box = InstancePool.Get<TickObjectBox>();
            TestObject1 to1 = InstancePool.Get<TestObject1>();
            TestObject2 to2 = InstancePool.Get<TestObject2>();
            Vec3 vec3 = InstancePool.Get<Vec3>();

            SnapshotDelta.GetRootAsSnapshotDelta(byteBuffer, ss);
            ss.GetTickObject(Box, 0);
            Debug.LogFormat("deserialize -> " + Box.TickObjectType);
            Box.GetTickObject(to1);
            to1.GetPos(vec3);
            Debug.LogFormat("pos({0}, {1}, {2})", vec3.X, vec3.Y, vec3.Z);
            to1.GetRot(vec3);
            Debug.LogFormat("rot({0}, {1}, {2})", vec3.X, vec3.Y, vec3.Z);
            ss.GetTickObject(Box, 1);
            Debug.LogFormat("deserialize -> " + Box.TickObjectType);
            Box.GetTickObject(to2);
            Debug.LogFormat("count({0})", to2.Count);
            ByteBufferPool.Dealloc(ref byteBuffer);
        }
    }
}
