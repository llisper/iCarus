using UnityEditor;

namespace iCarus.Coex
{
    [CustomEditor(typeof(CoexBehaviour), true)]
    class CoexBehaviourInspector : Editor
    {
        bool foldoutList;

        public override bool RequiresConstantRepaint()
        {
            return foldoutList;
        }

        public override void OnInspectorGUI()
        {
            CoexBehaviour script = (CoexBehaviour)target;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 95f;
            EditorGUILayout.LabelField("AddToEngine: " + script.addToEngine);
            if (script.coexCount > 0)
            {
                if (foldoutList = EditorGUILayout.Foldout(foldoutList, string.Format("Coroutines({0})", script.coexCount)))
                {
                    for (int p = 0; p < script.coexs.Length; ++p)
                    {
                        EditorGUILayout.LabelField(string.Format("{0}:", (CoexEngine.Process)p));
                        ++EditorGUI.indentLevel;
                        var list = script.coexs[p];
                        for (int i = 0; i < list.Count; ++i)
                        {
                            Coex c = list[i];
                            if (c.foldout = EditorGUILayout.Foldout(c.foldout, c.routineName))
                            {
                                if (null != c.returnValueType)
                                    EditorGUILayout.LabelField("return type:", c.returnValueType.FullName);
                                if (null != c.returnValue)
                                    EditorGUILayout.LabelField("last return:", c.returnValue.ToString());
                                EditorGUILayout.LabelField("yield count:", c.yieldCount.ToString());
                            }
                        }
                        --EditorGUI.indentLevel;
                    }
                }
            }
            EditorGUIUtility.labelWidth = labelWidth;
        }
    }
}