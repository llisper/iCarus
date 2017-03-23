using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

using System;
using System.Collections.Generic;

namespace iCarus.Singleton
{
    [CustomEditor(typeof(SingletonRoot))]
    class SingletonInspector : Editor
    {
        static Dictionary<string, bool> sFoldouts = new Dictionary<string, bool>();
        bool mConstantRepaint;

        public override bool RequiresConstantRepaint()
        {
            return mConstantRepaint;
        }

        public override void OnInspectorGUI()
        {
            ++EditorGUI.indentLevel;
            SingletonRoot script = (SingletonRoot)target;
            mConstantRepaint = false;
            for (int i = 0; i < script.singletonInstances.Count; ++i)
            {
                ISingleton inst = script.singletonInstances[i];
                if (inst is MonoBehaviour)
                    continue;

                Type type = inst.GetType();
                string name = inst.GetType().FullName;
                bool foldout = EditorGUILayout.Foldout(Foldout(name), name);
                if (foldout)
                {
                    SingletonRoot.IEditorDrawer drawer;
                    if (SingletonRoot.editorDrawers.TryGetValue(type, out drawer))
                    {
                        mConstantRepaint |= drawer.constantRepaint;
                        drawer.Draw(inst);
                    }
                }
                Foldout(name, foldout);
            }
            --EditorGUI.indentLevel;
        }

        [DidReloadScripts]
        static void RegisterDrawers()
        {
            // SingletonRoot.AddEditorDrawer<ObjectPoolManager>(new ObjectPoolManagerEditor());
        }

        static bool Foldout(string name)
        {
            bool foldout = false;
            if (!sFoldouts.TryGetValue(name, out foldout))
                sFoldouts.Add(name, foldout);
            return foldout;
        }

        static void Foldout(string name, bool value)
        {
            sFoldouts[name] = value;
        }
    }
}