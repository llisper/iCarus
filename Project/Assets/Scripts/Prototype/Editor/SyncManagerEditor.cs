/*
using UnityEditor;

namespace Prototype
{
    [CustomEditor(typeof(SyncManager))]
    class SyncManagerEditor : Editor
    {
        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            var script = (SyncManager)target;
            EditorGUILayout.Toggle("hasFullUpdated", script.hasFullUpdated);
            EditorGUILayout.IntField("serverTick", (int)script.serverTick, EditorStyles.label);
            EditorGUILayout.FloatField("simulateTicks", script.simulateTicks, EditorStyles.label);
            EditorGUILayout.IntField("snapshotCount", script.snapshotCount, EditorStyles.label);
            EditorGUILayout.IntField("cacheSnapshots", (int)script.cacheSnapshots, EditorStyles.label);
        }
    }
}
*/