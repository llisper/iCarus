using UnityEngine;
using UnityEngine.Rendering;

using System.Collections;

using iCarus.Log;
using iCarus.Coex;
using iCarus.Singleton;
using Foundation;

public class GameLog : Logging.Define<GameLog> { }

public class GameInitializer : MonoBehaviour
{
    public static GameInitializer Instance;

    public bool isHeadless { get { return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null; } }
    public bool Authorized { get; private set; }

    void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(this);
        StartCoroutine(Awaking());
    }

    void OnDestroy()
    {
        Instance = null;
    }

    IEnumerator Awaking()
    {
        if (isHeadless)
            yield return StartCoroutine(Logging.Initialize("Config/headless_log.xml"));
        else
            yield return StartCoroutine(Logging.Initialize("Config/log.xml"));
        GameLog.Info("Log Initialize");

        yield return StartCoroutine(Singletons.Add<AppConfig>("Config/config.json"));
        yield return StartCoroutine(Singletons.Add<CoexEngine>());

        UnityEngine.SceneManagement.SceneManager.LoadScene("PacMan");
    }
}
