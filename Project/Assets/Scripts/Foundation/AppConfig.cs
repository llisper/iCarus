using UnityEngine;

using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections;

using iCarus.Singleton;

namespace Foundation
{
    public sealed class AppConfig : Singleton<AppConfig>
    {
        #region configuration
        public int logPort = 11011;

        [Serializable]
        public class PacMan
        {
            public string appIdentifier = "Test";
            public string host = "localhost";
            public int port = 11012;
            public string map = "PacMan_1";
            public float beanRespawnInterval = 3f;
            public int beanTotal = 20;
            public float beanSpawnRadius = 20f;
            public int maxConnection = 2;
        }
        public PacMan pacMan = new PacMan();

        #endregion configuration

        string mConfigFileName;

        public AppConfig(string config)
        {
            mConfigFileName = config;
        }

        IEnumerator SingletonInit()
        {
            yield return GameInitializer.Instance.StartCoroutine(BytesReader.Read(mConfigFileName, bytes =>
            {
                if (null == bytes)
                {
                    #if UNITY_EDITOR_WIN || UNITY_STANDALONE
                    File.WriteAllText(
                        Path.Combine(Application.streamingAssetsPath, mConfigFileName),
                        JsonUtility.ToJson(Instance, true),
                        Encoding.UTF8);
                    #endif
                    GameLog.ErrorFormat("Read Config {0} Failed", mConfigFileName);
                }
                else
                {
                    // NOTE(llisper): detect BOM mark
                    // http://stackoverflow.com/questions/26101859/why-is-file-readallbytes-result-different-than-when-using-file-readalltext
                    string json;
                    if (bytes.Length > 3 &&
                        bytes[0] == 0xEF &&
                        bytes[1] == 0xBB &&
                        bytes[2] == 0xBF)
                    {
                        json = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
                    }
                    else
                    {
                        json = Encoding.UTF8.GetString(bytes);
                    }
                    JsonUtility.FromJsonOverwrite(json, Instance);
                }
            }));

            StringBuilder log = new StringBuilder("AppConfig:\n");
            LogConfig(log, "AppConfig", this);
            GameLog.Info(log.ToString());
        }

        void LogConfig(StringBuilder log, string name, object inst, int level = 0)
        {
            log.Append(' ', 3 * level);
            Type type = inst.GetType();
            if (type.IsPrimitive || inst is string)
            {
                log.AppendFormat(" - {0}: {1}\n", name, inst);
            }
            else
            {
                log.AppendFormat(" - {0}:\n", name);
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                    LogConfig(log, field.Name, field.GetValue(inst), level + 1);
            }
        }
    }
}
