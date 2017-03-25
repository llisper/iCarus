using UnityEngine;
using UnityEngine.SceneManagement;

namespace SimpleUI
{
    public class SceneList : MonoBehaviour
    {
        public void GotoScene(string scene)
        {
            UI.Instance.Close(GetType());
            SceneManager.LoadScene(scene);
        }
    }
}
