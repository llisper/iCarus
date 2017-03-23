using UnityEngine;

using System;
using System.Reflection;
using System.Collections;

namespace iCarus.Singleton
{
    class SingletonLog : Log.Logging.Define<SingletonLog> { }

    public class SingletonException : Exception { }

    public interface ISingleton
    {
        void DestroySingleton();
    }

    /// <summary>
    /// 普通单件继承这个类, 并且标记为sealed
    /// </summary>
    /// <typeparam name="T">单件类型</typeparam>
    public class Singleton<T> : ISingleton
    {
        internal static T sInstance = default(T);
        public static T Instance { get { return sInstance; } }

        public virtual void DestroySingleton()
        {
            Singletons.Remove(this);
            sInstance = default(T);
        }
    }

    /// <summary>
    /// Unity使用MonoBehaviour的单件继承这个类, 并且标记为sealed
    /// </summary>
    /// <typeparam name="T">单件类型</typeparam>
    public class SingletonBehaviour<T> : MonoBehaviour, ISingleton
    {
        internal static T sInstance = default(T);
        public static T Instance { get { return sInstance; } }

        public virtual void DestroySingleton()
        {
            Singletons.Remove(this);
            sInstance = default(T);
        }
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
        /// <typeparam name="T">单件类型</typeparam>
        /// <param name="args">传递给单件构造函数的参数, 对MonoBehaviour不适用</param>
        /// <returns>迭代器</returns>
        public static IEnumerator Add<T>(params object[] args)
        {
            return Add(typeof(T), args);
        }

        /// <summary>
        /// 增加一个单件
        /// </summary>
        /// <param name="type">单件类型</param>
        /// <param name="args">传递给单件构造函数的参数, 对MonoBehaviour不适用</param>
        /// <returns>迭代器</returns>
        public static IEnumerator Add(Type type, params object[] args)
        {
            BaseType baseType = GetBaseType(type);
            if (baseType == BaseType.None ||
                !type.IsSealed ||
                type.IsAbstract ||
                type.IsGenericType)
            {
                Exception.Throw<SingletonException>("Type {0} is not qualify for a singleton", type.FullName);
            }

            ISingleton inst = null;
            if (BaseType.SingletonBehaviour == baseType)
            {
                GameObject go = new GameObject(type.Name, type);
                go.transform.parent = root.transform;
                go.transform.Reset();
                inst = (ISingleton)go.GetComponent(type);
            }
            else if (BaseType.Singleton == baseType)
            {
                inst = (ISingleton)Activator.CreateInstance(type, args);
            }

            if (null == inst)
                Exception.Throw<SingletonException>("Failed to create singleton: " + type.FullName);

            type.InvokeMember(
                "sInstance",
                BindingFlags.FlattenHierarchy | BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Static,
                null,
                null,
                new object[] { inst });

            object ret = CallSingletonMethod("SingletonInit", inst, inst.GetType());
            if (null != ret && ret is IEnumerator)
                yield return root.StartCoroutine((IEnumerator)ret);

            root.singletonInstances.Add(inst);
            SingletonLog.InfoFormat("{0} Added", type.FullName);
            yield return inst;
        }

        public static void Remove(ISingleton singleton)
        {
            if (root.singletonInstances.Remove(singleton))
            {
                if (singleton is MonoBehaviour)
                    GameObject.Destroy(((MonoBehaviour)singleton).gameObject);
                SingletonLog.InfoFormat("{0} Removed", singleton.GetType().FullName);
            }
        }

        /// <summary>
        /// 对所有单件进行初始化, 对所有单件调用SingletonStart, 如果SingletonStart, 则使用协程等待
        /// </summary>
        /*
        public static IEnumerator Start()
        {
            SingletonLog.InfoFormat("Start Singletons({0}):", root.singletonInstances.Count);
            for (int i = 0; i < root.singletonInstances.Count; ++i)
            {
                var inst = root.singletonInstances[i];
                object ret = CallSingletonMethod("SingletonStart", inst, inst.GetType());
                if (null != ret && ret is IEnumerator)
                    yield return root.StartCoroutine((IEnumerator)ret);
                SingletonLog.InfoFormat("{0} Started", inst.GetType().FullName);
            }
            SingletonLog.InfoFormat("Done!");
        }
        */

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

        static object CallSingletonMethod(string methodName, object inst, Type type)
        {
            if (type == typeof(object))
                return null;

            if (type.IsGenericType)
            {
                if (type.GetGenericTypeDefinition() == typeof(Singleton<>) ||
                    type.GetGenericTypeDefinition() == typeof(SingletonBehaviour<>))
                {
                    return null;
                }
            }

            MethodInfo method = type.GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (null != method)
                return method.Invoke(inst, null);
            else
                return CallSingletonMethod(methodName, inst, type.BaseType);
        }
    }
}
