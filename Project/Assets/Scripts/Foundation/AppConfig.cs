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
        public float simulatedDuplicatesChance = 0f;
        public float simulatedLoss = 0f;
        public float simulatedMinimumLatency = 0f;
        public float simulatedRandomLatency = 0f;

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

        public float tickrate = 0.015f;   // 66.6 t/s
        public float updaterate = 0.045f; // 45 ms/snapshot, do snapshot every 3 ticks
        public float cmdrate = 0.03f;    // 33.3 p/s send input every 2 ticks
        public uint cachesnapshots = 2;  // cache number of snapshots before simulate
        public int defaultOutgoingMessageCapacity = 4096;
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
