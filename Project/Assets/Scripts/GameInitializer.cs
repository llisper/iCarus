using UnityEngine;
using UnityEngine.Rendering;

using System.Collections;

using iCarus.Log;
using iCarus.Coex;
using iCarus.Singleton;
using Foundation;

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
        yield return StartCoroutine(InitializeLog());
        yield return StartCoroutine(Singletons.Add<AppConfig>("Config/config.json"));
        yield return StartCoroutine(Singletons.Add<CoexEngine>());

        UnityEngine.SceneManagement.SceneManager.LoadScene("PacMan");
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
