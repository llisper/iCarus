using UnityEngine;
using System;
using System.Collections.Generic;
using iCarus;
using Protocol;
using Prototype.Common;
using FlatBuffers;

namespace Prototype.Game
{
    public class AvatarData
    {
        public Color color;
    }

    public class AvatarClient : ITickObjectClient
    {
        public abstract class AvatarComponent : ITickObjectClient
        {
            public int id { get; private set; }
            public AvatarClient avatar { get; private set; }
            public abstract void FullUpdate(TickObject obj);
            public abstract void EventUpdate(TickEvent evt);
            public abstract void Lerping(float t, TickObject obj);

            public abstract bool predict { get; }
            public abstract void ApplyDeltaForPredict(TickObject obj);
            public abstract void Predict();

            public abstract IDictionary<int, ITickObjectClient> children { get; }

            public virtual void Init(int id, AvatarClient avatar)
            {
                this.id = id;
                this.avatar = avatar;
            }
        }

        public int id { get; private set; }
        public bool predict { get { return false; } }
        public bool local { get; private set; }
        public IDictionary<int, ITickObjectClient> children { get { return mComponents; } }
        public AvatarCommon common { get { return mCommon; } }

        AvatarCommon mCommon;
        GameObject mGameObject;
        KinematicsClient mKinematics;
        Dictionary<int, ITickObjectClient> mComponents = new Dictionary<int, ITickObjectClient>();

        public static AvatarClient New(int id, bool local, Transform parent)
        {
            AvatarClient avatar = new AvatarClient();
            avatar.id = id;
            avatar.local = local;
            var prefab = Resources.Load("Prototype/Avatar");
            GameObject go = (GameObject)GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
            avatar.mGameObject = go;
            avatar.mCommon = go.GetComponent<AvatarCommon>();
            avatar.mKinematics = avatar.AddComponent<KinematicsClient>(0);
            SyncManagerClient.Instance.Add(avatar);
            return avatar;
        }

        public void FullUpdate(TickObject obj)
        {
            Protocol.Avatar data = InstancePool.Get<Protocol.Avatar>();
            obj.GetData(data);
            mCommon.color = (new Color()).FromInt(data.Color);
        }

        public void EventUpdate(TickEvent evt) { }
        public void Lerping(float t, TickObject obj) { }
        public void ApplyDeltaForPredict(TickObject obj) { }
        public void Predict() { }

        T AddComponent<T>(int id) where T : AvatarComponent, new()
        {
            T instance = new T();
            instance.Init(id, this);
            mComponents.Add(id, instance);
            return instance;
        }
    }
}
