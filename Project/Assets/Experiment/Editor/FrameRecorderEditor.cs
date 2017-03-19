using UnityEngine;
using UnityEditor;

namespace Experimental
{
    [CustomEditor(typeof(FrameRecorder))]
    public class FrameSnapshotEditor : Editor
    {
        public override bool RequiresConstantRepaint()
        {
            return true;
        }

        bool foldoutFrames;

        public override void OnInspectorGUI()
        {
            FrameRecorder script = (FrameRecorder)target;

            EditorGUILayout.LabelField("current frames: " + script.frameData.Count);
            EditorGUILayout.LabelField("histories:");
            for (int i = 0; i < script.histories.Count; ++i)
                EditorGUILayout.LabelField(string.Format("[{0}] frames: {1}", i, script.histories[i].Count));

            foldoutFrames = EditorGUILayout.Foldout(foldoutFrames, "frame objects " + script.objects.Count);
            if (foldoutFrames)
            {
                foreach (var frame in script.objects)
                {
                    if (frame is MonoBehaviour)
                        EditorGUILayout.ObjectField((MonoBehaviour)frame, frame.GetType(), false);
                    else
                        EditorGUILayout.LabelField(frame.GetType().Name);
                }
            }
        }
    }
}
