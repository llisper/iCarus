/*
using UnityEditor;

namespace Prototype
{
    [CustomEditor(typeof(TServer))]
    class TServerEditor : Editor
    {
        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            TServer script = (TServer)target;
            EditorGUILayout.IntField("tickCount", (int)script.tickCount, EditorStyles.label);
            EditorGUILayout.IntField("snapshotOverTick", (int)script.snapshotOverTick, EditorStyles.label);

            for (int i = 0; i < script.players.Count; ++i)
            {
                var p = script.players[i];
                EditorGUILayout.IntField(
                    string.Format("{0}.choke", p.name),
                    p.choke,
                    EditorStyles.label);
            }

            if (null != script.netServer)
            {
                EditorGUILayout.TextArea(script.netServer.Statistics.ToString(), EditorStyles.label);
                foreach (var conn in script.netServer.Connections)
                {
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