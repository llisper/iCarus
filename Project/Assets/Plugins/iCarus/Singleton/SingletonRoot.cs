using UnityEngine;
using System;
using System.Collections.Generic;

namespace iCarus.Singleton
{
    /// <summary>
    /// 所有Unity单件的父节点, 主要用于在编辑器中显示一些单件的信息辅助调试
    /// </summary>
    public class SingletonRoot : MonoBehaviour
    {
        /// <summary>
        /// 在编辑器中绘制的接口, 由Editor下的脚本使用[DidReloadScripts]标记的函数进行注册
        /// 只有继承自Singleton<>的单件才需要使用这种方式来在编辑器中绘制, 继承自SingletonBehaviour<>的可以直接定制编辑器
        /// 如果需要使用但是不明白上面的描述, 可以问@llisper
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal interface IEditorDrawer
        {
            bool constantRepaint { get; }
            void Draw(ISingleton inst);
        }

        internal List<ISingleton> singletonInstances = new List<ISingleton>();
        internal static Dictionary<Type, IEditorDrawer> editorDrawers = new Dictionary<Type, IEditorDrawer>();

        internal static void AddEditorDrawer<T>(IEditorDrawer drawer)
        {
            Type type = typeof(T);
            if (!editorDrawers.ContainsKey(type))
                editorDrawers.Add(type, drawer);
        }
    }
}
