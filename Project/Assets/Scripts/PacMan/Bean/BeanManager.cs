using UnityEngine;
using System.Collections.Generic;
using iCarus.Singleton;
using Foundation;

namespace PacMan
{
    public sealed class BeanManager : SingletonBehaviour<BeanManager>, ISimulate, INetSync
    {
        public int id { get { return NetId.BeanManager; } }

        UnityEngine.Object mBeanPrefab;
        List<Bean> mBeans = new List<Bean>();

        void SingletonInit()
        {
            mBeanPrefab = Resources.Load("PacMan/Bean");
            SimulateManager.Instance.Add(this);
        }

        public override void DestroySingleton()
        {
            mBeans.Clear();
            base.DestroySingleton();
        }

        public void Init()
        {
            for (int i = 0; i < AppConfig.Instance.pacMan.beanTotal; ++i)
            {
                Vector3 position;
                int pos = SceneManager.Instance.spawnpoints.Next(out position);
                GameObject go = (GameObject)Instantiate(mBeanPrefab, transform);
                go.transform.position = position;
                Bean bean = new Bean(NetId.Instance.NextBean(), pos, go.GetComponent<BeanView>());
                bean.onEaten = OnBeanEaten;
                // TODO: 
                //  add bean to sync-manager
                mBeans.Add(bean);
            }
        }

        public void SimulateFixedUpdate()
        {

        }

        void OnBeanEaten(Bean bean, Player player)
        {
            mBeans.Remove(bean);
            SceneManager.Instance.spawnpoints.Release(bean.pos);
            bean.Dispose();
        }
    }
}
