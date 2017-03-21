using UnityEngine;

using System;
using System.Collections.Generic;

namespace SimpleUI
{
    public class UI : MonoBehaviour
    {
        public static UI Instance;

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        void OnDestroy()
        {
            Instance = null;
        }

        Dictionary<Type, Component> mPanels = new Dictionary<Type, Component>();

        public T Show<T>() where T : Component
        {
            Type type = typeof(T);
            Component panel = null;
            if (!mPanels.TryGetValue(type, out panel))
            {
                UnityEngine.Object prefab = Resources.Load("UI/" + type.Name);
                if (null != prefab)
                {
                    GameObject go = (GameObject)Instantiate(prefab, transform);
                    if (null != go)
                    {
                        panel = go.GetComponent(type);
                        if (null != panel)
                            mPanels.Add(type, panel);
                        else
                            Destroy(go);
                    }
                }
            }
            return (T)panel;
        }

        public void Close<T>()
        {
            Component panel;
            if (mPanels.TryGetValue(typeof(T), out panel))
            {
                Destroy(panel.gameObject);
                mPanels.Remove(typeof(T));
            }
        }

        public void CloseAll()
        {
            foreach (var kv in mPanels)
                Destroy(kv.Value.gameObject);
            mPanels.Clear();
        }
    }
}