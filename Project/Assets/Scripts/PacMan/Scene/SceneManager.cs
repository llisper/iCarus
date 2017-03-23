using UnityEngine;
using iCarus;
using iCarus.Singleton;

namespace PacMan
{ 
    public sealed class SceneManager : Singleton<SceneManager>
    { 
        public BeanSpawnpoints spawnpoints;

        public void Load(string map)
        {
            string path = "PacMan/Map/" + map;
            GameLog.InfoFormat("Loading map [{0}]...", path);

            UnityEngine.Object prefab = Resources.Load(path);
            if (null == prefab)
                iCarus.Exception.Throw<GameException>("Load map failed: " + path);

            GameObject go = (GameObject)GameObject.Instantiate(prefab);
            go.transform.Reset();
            spawnpoints = go.GetComponent<BeanSpawnpoints>();
            if (null == spawnpoints)
            {
                string n = go.name;
                GameObject.Destroy(go);
                iCarus.Exception.Throw<GameException>("<BeanSpawnpoints> is not found on " + n);
            }

            GameLog.InfoFormat("Map [{0}] loaded", path);
        }

        public override void DestroySingleton()
        {
            base.DestroySingleton();
            /*
            if (null != spawnpoints)
            {
                GameObject.Destroy(spawnpoints.gameObject);
                spawnpoints = null;
            }
            */
        }
    }
}
