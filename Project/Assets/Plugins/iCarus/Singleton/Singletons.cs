using UnityEngine;

using System;
using System.Reflection;
using System.Collections.Generic;

namespace iCarus.Singleton
{
    public class SingletonLog : Log.Logging.Define<SingletonLog> { }
    public class SinletonException : Exception { }

    /// <summary>
    /// 普通单件继承这个类, 并且标记为sealed
    /// </summary>
    /// <typeparam name="T">单件类型</typeparam>
    public class Singleton<T>
    {
        internal static T sInstance = default(T);
        public static T Instance { get { return sInstance; } }
    }

    /// <summary>
    /// Unity使用MonoBehaviour的单件继承这个类, 并且标记为sealed
    /// </summary>
    /// <typeparam name="T">单件类型</typeparam>
    public class SingletonBehaviour<T> : MonoBehaviour
    {
        internal static T sInstance = default(T);
        public static T Instance { get { return sInstance; } }
    }

    /// <summary>
    /// 单件初始化工具
    /// </summary>
    public static class Singletons
    {
        static SingletonRoot sRoot;

        static SingletonRoot root
        {
            get
            {
                if (null == sRoot)
                {
                    sRoot = GameObject.FindObjectOfType<SingletonRoot>();
                    if (null == sRoot)
                    {
                        GameObject go = new GameObject("SingletonRoot", typeof(SingletonRoot));
                        go.transform.Reset();
                        if (Application.isPlaying)
                            GameObject.DontDestroyOnLoad(go);
                        sRoot = go.GetComponent<SingletonRoot>();
                    }
                }
                return sRoot;
            }
        }

        /// <summary>
        /// 增加一个单件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Add<T>() where T : new()
        {
            Type type = typeof(T);
            BaseType baseType = GetBaseType(type);
            if (baseType == BaseType.None ||
                !type.IsSealed ||
                type.IsAbstract ||
                type.IsGenericType)
            {
                Exception.Throw<SinletonException>("Type {0} is not qualify for a singleton", type.FullName);
            }

            object inst = null;
            if (BaseType.SingletonBehaviour == baseType)
            {
                GameObject go = new GameObject(type.Name, type);
                go.transform.parent = root.transform;
                go.transform.Reset();
                inst = go.GetComponent(type);
            }
            else if (BaseType.Singleton == baseType)
            {
                inst = Activator.CreateInstance(type);
            }

            if (null != inst)
            {
                DoInitializeSingleton(inst);
                root.singletonInstances.Add(inst);
            }
        }

        /// <summary>
        /// 对所有单件进行初始化, 初始化流程是:
        /// 1. 对所有单件调用SingletonAwake函数
        /// 2. 对所有单件调用SinletonStart函数
        /// </summary>
        public static void Initialize()
        {
            DoSingletonAwake(root.singletonInstances);
            SingletonLog.InfoFormat("Initialize {0} singletons", root.singletonInstances.Count);
        }

        enum BaseType
        {
            None,
            SingletonBehaviour,
            Singleton,
        }

        static BaseType GetBaseType(Type type)
        {
            if (null != type.BaseType && type.BaseType.IsGenericType)
            {
                if (type.BaseType.GetGenericTypeDefinition() == typeof(SingletonBehaviour<>))
                    return BaseType.SingletonBehaviour;
                else if (type.BaseType.GetGenericTypeDefinition() == typeof(Singleton<>))
                    return BaseType.Singleton;
                else
                    return GetBaseType(type.BaseType);
            }
            return BaseType.None;
        }

        static void DoSingletonAwake(List<object> instances)
        {
            for (int i = 0; i < instances.Count; ++i)
                CallSingletonMethod("SingletonAwake", instances[i]);

            for (int i = 0; i < instances.Count; ++i)
                CallSingletonMethod("SingletonStart", instances[i]);
        }

        static void CallSingletonMethod(string methodName, object inst)
        {
            CallSingletonMethod(methodName, inst, inst.GetType());
        }

        static void CallSingletonMethod(string methodName, object inst, Type type)
        {
            if (type == typeof(object))
                return;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Singleton<>) ||
                    type.GetGenericTypeDefinition() == typeof(SingletonBehaviour<>))
                {
                    return;
                }
            }

            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (null != method)
                method.Invoke(inst, null);
            else
                CallSingletonMethod(methodName, inst, type.BaseType);
        }

        internal static void DoInitializeSingleton(object inst)
        {
            Type type = inst.GetType();
            type.InvokeMember(
                "sInstance",
                BindingFlags.FlattenHierarchy | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                null, 
                new object[] { inst });
        }
    }
}
