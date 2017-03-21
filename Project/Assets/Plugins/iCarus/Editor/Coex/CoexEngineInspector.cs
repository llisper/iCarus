using UnityEditor;

namespace iCarus.Coex
{
    [CustomEditor(typeof(CoexEngine))]
    class CoexEngineInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            CoexEngine script = (CoexEngine)target;
            var bvrs = script.behaviours;
            for (int i = 0; i < bvrs.Count; ++i)
            {
                EditorGUILayout.ObjectField(
                    string.Format("{0}.", i),
                    bvrs[i],
                    typeof(CoexBehaviour),
                    false);
            }
        }
    }
}