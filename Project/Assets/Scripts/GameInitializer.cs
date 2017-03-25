using UnityEngine;
using UnityEngine.Rendering;

using System;
using System.Collections;

using iCarus.Log;
using iCarus.Coex;
using iCarus.Network;
using iCarus.Singleton;
using SimpleUI;
using Protocol;
using Foundation;
using FlatBuffers;

public class GameException : iCarus.Exception { }
public class GameLog : Logging.Define<GameLog> { }

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance;

    public bool isHeadlessOverride = false;
    public bool isHeadless
    {
        get
        {
            return isHeadlessOverride || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
        }
    }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        StartCoroutine(AwakeRoutine());
    }

    void OnDestroy()
    {
        Instance = null;
        #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        ConsoleWindow.Instance.Shutdown();
        #endif
    }

    IEnumerator AwakeRoutine()
    {
        FlatBuffersInitializer.Initialize(typeof(ProtocolInitializer).Assembly);
        MessageBuilder.Initialize();
        yield return StartCoroutine(InitializeLog());
        yield return StartCoroutine(Singletons.Add<AppConfig>("Config/config.json"));
        yield return StartCoroutine(Singletons.Add<CoexEngine>());

        if (!isHeadless)
        {
            UI.Instance.Show<SceneList>();
        }
        else
        {
            string scene = null;
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "--scene" && i < args.Length - 1)
                {
                    scene = args[i + 1];
                    break;
                }
            }

            if (null != scene)
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
            }
            else
            {
                GameLog.Error("specify scene on command line, eg. --scene PacMan");
                Application.Quit();
            }
        }
    }

    IEnumerator InitializeLog()
    {
        if (isHeadless)
        {
            #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            ConsoleWindow.Instance.Initialize();
            ConsoleWindow.Instance.SetTitle("DedicatedServer");
            #endif
            yield return StartCoroutine(Logging.Initialize("Config/headless_log.xml"));
        }
        else
        {
            yield return StartCoroutine(Logging.Initialize("Config/log.xml"));
        }
        GameLog.Info("Log Initialize");
    }
}
