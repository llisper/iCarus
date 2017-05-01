using UnityEditor;

namespace Prototype.Server
{
    [CustomEditor(typeof(Server))]
    class ServerEditor : Editor
    {
        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            Server script = (Server)target;
            var net = script.netlayer.netServer;
            if (null != net)
            {
                EditorGUILayout.TextArea(net.Statistics.ToString(), EditorStyles.label);
                foreach (var conn in net.Connections)
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