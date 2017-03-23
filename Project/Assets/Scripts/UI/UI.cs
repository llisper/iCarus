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
            return (T)Show(typeof(T));
        }

        public Component Show(Type type)
        {
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
            return panel;
        }

        public void Close<T>()
        {
            Close(typeof(T));
        }

        public void Close(Type type)
        {
            Component panel;
            if (mPanels.TryGetValue(type, out panel))
            {
                Destroy(panel.gameObject);
                mPanels.Remove(type);
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