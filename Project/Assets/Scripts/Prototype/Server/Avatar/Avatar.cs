using UnityEngine;
using System.Collections.Generic;
using iCarus;
using Protocol;
using FlatBuffers;
using Prototype.Common;

namespace Prototype.Server
{
    public class Avatar : ITickObject
    {
        public abstract class AvatarComponent : ITickObject
        {
            public int id { get; private set; }
            public Avatar avatar { get; private set; }
            public abstract TickObjectType type { get; }
            public abstract TickEventType eventType { get; }
            public abstract IList<ITickObject> children { get; }

            public abstract void Simulate();
            public abstract int  Snapshot(FlatBufferBuilder fbb, bool full);
            public abstract int  SnapshotEvent(FlatBufferBuilder fbb, uint tickCount);

            public void Init(int id, Avatar avatar)
            {
                this.id = id;
                this.avatar = avatar;
            }
        }

        public int id { get; private set; }
        public TickObjectType type { get { return TickObjectType.Avatar; } }
        public TickEventType eventType { get { return TickEventType.NONE; } }
        public IList<ITickObject> children { get { return mComponents; } }
        public AvatarCommon common { get { return mCommon; } }
        public InputProvider inputProvider { get { return mInputProvider; } }

        AvatarCommon mCommon;
        GameObject mGameObject;
        Kinematics mKinematics;
        InputProvider mInputProvider;
        List<ITickObject> mComponents = new List<ITickObject>();

        public static Avatar New(int id, Transform parent)
        {
            Avatar avatar = new Avatar();
            avatar.id = id;
            var prefab = Resources.Load("Prototype/Avatar");
            GameObject go = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            avatar.mGameObject = go;
            avatar.mCommon = go.GetComponent<AvatarCommon>();
            avatar.mKinematics = avatar.AddComponent<Kinematics>(0);
            SyncManager.Instance.Add(avatar);
            return avatar;
        }

        public void SetInputProvider(InputProvider inputProvider)
        {
            mInputProvider = inputProvider;
        }

        public void Simulate()
        {
            mInputProvider.MoveNext();
        }

        public int Snapshot(FlatBufferBuilder fbb, bool full)
        {
            if (full)
                return Protocol.Avatar.CreateAvatar(fbb, mCommon.color.ToInt()).Value;
            else
                return 0;
        }

        public int SnapshotEvent(FlatBufferBuilder fbb, uint tickCount)
        {
            return 0;
        }

        T AddComponent<T>(int id) where T : AvatarComponent, new()
        {
            T instance = new T();
            instance.Init(id, this);
            mComponents.Add(instance);
            return instance;
        }
    }
}
