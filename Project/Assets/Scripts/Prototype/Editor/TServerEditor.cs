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
        }
    }
}