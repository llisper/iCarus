using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Experimental
{
    public class Game : MonoBehaviour
    {
        public List<GameObject> prefabs;

        [NonSerialized]
        public InputSampler inputSampler;
        [NonSerialized]
        public List<IRecorder> recorders = new List<IRecorder>();

        FrameRecorder _frameRecorder;
        public FrameRecorder frameRecorder
        {
            get
            {
                if (null == _frameRecorder)
                    _frameRecorder = GetRecorder<FrameRecorder>();
                return _frameRecorder;
            }
        }

        TrailRecorder _trailRecorder;
        public TrailRecorder trailRecorder
        {
            get
            {
                if (null == _trailRecorder)
                    _trailRecorder = GetRecorder<TrailRecorder>();
                return _trailRecorder;
            }
        }

        public static Game instance;
        void Awake() { instance = this; }
        void OnDestroy() { instance = null; }

        void Start()
        {
            AddRecorder<FrameRecorder>();
            AddRecorder<TrailRecorder>();
            inputSampler = gameObject.AddComponent<InputSampler>();

            foreach (var prefab in prefabs)
                GameObject.Instantiate(prefab, transform);
        }

        void AddRecorder<T>() where T : MonoBehaviour, IRecorder
        {
            recorders.Add(gameObject.AddComponent<T>());
        }

        T GetRecorder<T>() where T : IRecorder
        {
            return (T)recorders.Find(r => r is T);
        }

        public void SubscribeAll(IFrame frame)
        {
            foreach (var r in recorders)
                r.Subscribe(frame);
        }

        public void UnsubscribeAll(IFrame frame)
        {
            foreach (var r in recorders)
                r.Unsubscribe(frame);
        }

        public void ReloadScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void StartGame()
        {
            foreach (var r in recorders)
                r.recording = true;
        }

        public void StopGame()
        {
            foreach (var r in recorders)
                r.recording = false;
        }

        public void Rewind(int startFrame)
        {
            frameRecorder.RewindTo(startFrame);
        }
    }
}
