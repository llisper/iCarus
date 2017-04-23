using UnityEditor;
using System.Text;

namespace Prototype
{
    [CustomEditor(typeof(SyncManager))]
    class SyncManagerEditor : Editor
    {
        SyncManager script;

        public override bool RequiresConstantRepaint() { return true; }

        public override void OnInspectorGUI()
        {
            script = (SyncManager)target;
            Info();
        }

        void Info()
        {
            StringBuilder text = new StringBuilder();
            text.AppendFormat("tickrate: {0}\n", script.tickrate)
                .AppendFormat("updaterate: {0}\n", script.updaterate)
                .AppendFormat("snapshotOverTick: {0}\n", script.snapshotOverTick)
                .AppendFormat("inputchoke: {0}\n", script.inputchoke)
                .AppendFormat("tickCount: {0}\n", script.tickCount)
                .AppendFormat("tickObjects: {0}\n", script.mTickObjects.Count);
            EditorGUILayout.LabelField(text.ToString(), EditorStyles.textArea);
        }
    }
}