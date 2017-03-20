using System.Collections.Generic;
using UnityEngine;

namespace Experimental
{
    public class Recorder<T> : MonoBehaviour, IRecorder
    {
        bool mRecording = false;
        public virtual bool recording
        {
            get { return mRecording; }
            set { mRecording = value; }
        }

        public List<T> objects = new List<T>();

        public void Subscribe(object obj)
        {
            if (!objects.Contains((T)obj))
                objects.Add((T)obj);
        }

        public void Unsubscribe(object obj)
        {
            objects.Remove((T)obj);
        }

        public void Clear()
        {
            objects.Clear();
        }
    }
}
