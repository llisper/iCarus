using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Experimental
{
    [CustomEditor(typeof(Game))]
    public class GameEditor : Editor
    {
        int rewindToFrame;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            // WidthHelper();

            Game script = (Game)target;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("reload", EditorStyles.miniButton, GUILayout.Width(70f)))
                script.ReloadScene();
            if (GUILayout.Button("start", EditorStyles.miniButton, GUILayout.Width(70f)))
                script.StartGame();
            if (GUILayout.Button("stop", EditorStyles.miniButton, GUILayout.Width(70f)))
                script.StopGame();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            rewindToFrame = EditorGUILayout.IntField(rewindToFrame, GUILayout.Width(144f));
            if (GUILayout.Button("rewind", EditorStyles.miniButton, GUILayout.Width(70f)))
                script.Rewind(rewindToFrame);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("compare histories", EditorStyles.miniButton, GUILayout.Width(218f)))
            {
                try
                {
                    CompareHistories(script.frameRecorder);
                } catch (Exception) { }
                EditorUtility.ClearProgressBar();
            }
        }

        int Total(IList list)
        {
            int count = 0;
            foreach (var val in list)
            {
                if (val is IList)
                    count += Total((IList)val);
                else
                    ++count;
            }
            return count;
        }

        void CompareHistories(FrameRecorder recorder)
        {
            var histories = recorder.histories;
            var frameData = recorder.frameData;
            StringBuilder log = new StringBuilder(1024 * 1024);

            int processed = 0;
            int total = Total(histories);
            bool allGood = true;
            for (int i = histories.Count - 1; i >= 0; --i)
            {
                List<List<FrameRecorder.FrameData>> older = histories[i];
                List<List<FrameRecorder.FrameData>> newer = i > 0 ? histories[i - 1] : frameData;
                int frameCount = Mathf.Min(older.Count, newer.Count);

                bool good = true;
                for (int j = 0; j < frameCount; ++j)
                {
                    List<FrameRecorder.FrameData> olderList = older[j];
                    List<FrameRecorder.FrameData> newerList = newer[j];

                    if (olderList.Count != newerList.Count)
                    {
                        good &= false;
                        log.AppendFormat(
                            "Error[fd_list:{0}]: {1} -> {2}, frame data list count not equal\n",
                            j, i, i > 0 ? (i - 1).ToString() : "current");
                        continue;
                    }

                    for (int k = 0; k < olderList.Count; ++k, ++processed)
                    {
                        FrameRecorder.FrameData ofd = olderList[k];
                        FrameRecorder.FrameData nfd = newerList.Find(v => v.frameObj == ofd.frameObj);

                        EditorUtility.DisplayCancelableProgressBar(
                            "compare histories",
                            string.Format("working...{0}/{1},{2}/{3},{4}/{5}", histories.Count - i - 1, histories.Count, j, frameCount, k, olderList.Count),
                            (float)processed / total);

                        if (null == nfd)
                        {
                            good &= false;
                            log.AppendFormat(
                                "Error[fd_list:{0}]: {1} -> {2}, {3} is not found in newer list\n",
                                j, i, i > 0 ? (i - 1).ToString() : "current",
                                ofd.frameObj.identity);
                            continue;
                        }

                        if (!ofd.frameData.Compare(nfd.frameData))
                        {
                            good &= false;
                            log.AppendFormat(
                                "Error[fd_list:{0}]: {1} -> {2}, {3}'s frame data({4}) is not equal with newer counterpart\n",
                                j, i, i > 0 ? (i - 1).ToString() : "current",
                                ofd.frameObj.identity,
                                ofd.frameData.GetType().Name);
                        }
                    }
                }
                allGood &= good;
            }

            string logPath = Application.dataPath + "/../compare_histories.log";
            if (File.Exists(logPath))
                File.Delete(logPath);
            if (log.Length > 0)
                File.WriteAllText(logPath, log.ToString(), Encoding.UTF8);
            Debug.LogFormat("All Done: <b><color={0}>{1}</color></b>, view log at {2}", allGood ? "green" : "red", allGood, logPath);
        }

        float w;
        void WidthHelper()
        {
            w = EditorGUILayout.FloatField("w", w);
        }
    }
}
