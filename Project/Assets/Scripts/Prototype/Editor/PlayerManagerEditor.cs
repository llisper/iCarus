using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.Collections.Generic;

namespace Prototype
{
    [CustomEditor(typeof(PlayerManager))]
    public class PlayerManagerEditor : Editor
    {
        PlayerManager script;
        bool constantUpdate;

        public override bool RequiresConstantRepaint()
        {
            return constantUpdate;
        }

        public override void OnInspectorGUI()
        {
            script = (PlayerManager)target;
            AuthList();
            PlayerList();
        }

        void AuthList()
        {
            if (Foldout.Check("auth list"))
            {
                foreach (var ac in script.mAuthingConnections)
                    EditorGUILayout.LabelField(string.Format("{0} > {1}", ac.timer, ac.connection.RemoteEndPoint));
            }
        }

        void PlayerList()
        {
            constantUpdate = false;
            if (Foldout.Check("player list"))
            {
                ++EditorGUI.indentLevel;
                foreach (var player in script.players)
                {
                    if (Foldout.Check(player.playerName))
                    {
                        StringBuilder text = new StringBuilder();
                        text.AppendFormat("id: {0}\n", player.id)
                            .AppendFormat("state: {0}\n", player.state)
                            .AppendFormat("choke: {0}\n", player.choke)
                            .Append("ackInput: " + string.Join(",", Array.ConvertAll(player.mAckInputs, v => v.ToString())));
                        EditorGUILayout.LabelField(text.ToString(), EditorStyles.textArea, GUILayout.Width(300f));
                        constantUpdate = true;
                    }
                }
                --EditorGUI.indentLevel;
            }
        }
    }

    public class Foldout
    {
        public static bool Check(string name)
        {
            bool value;
            if (!mFoldouts.TryGetValue(name, out value))
                mFoldouts.Add(name, value);
            value = EditorGUILayout.Foldout(value, name);
            mFoldouts[name] = value;
            return value;
        }

        static Dictionary<string, bool> mFoldouts = new Dictionary<string, bool>();
    }
}