/*
using UnityEditor;

namespace Prototype
{
    [CustomEditor(typeof(TClient))]
    class TClientEditor : Editor
    {
        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            TClient script = (TClient)target;
            EditorGUILayout.LabelField("notAckInputs: " + script.input.inputQueue.Count);
            if (null != script.netClient)
            {
                EditorGUILayout.TextArea(script.netClient.Statistics.ToString(), EditorStyles.label);
                if (null != script.netClient.ServerConnection)
                {
                    var conn = script.netClient.ServerConnection;
                    EditorGUILayout.LabelField(conn.ToString(), EditorStyles.boldLabel);
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.TextArea(conn.Statistics.ToString(), EditorStyles.label);
                    --EditorGUI.indentLevel;
                }
            }
        }
    }
}
*/