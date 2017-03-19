using UnityEngine;

using System.Collections;
using System.Collections.Generic;

namespace Experimental
{
    public class FrameRecorder : Recorder<IFrame>
    {
        public class FrameData
        {
            public IFrame frameObj;
            public IFrameData frameData;
        }

        public List<List<FrameData>> frameData = new List<List<FrameData>>();
        public List<List<List<FrameData>>> histories = new List<List<List<FrameData>>>();

        void FixedUpdate()
        {
            if (recording)
            {
                List<FrameData> list = new List<FrameData>();
                foreach (var frameObj in objects)
                {
                    list.Add(new FrameData()
                    {
                        frameObj = frameObj,
                        frameData = frameObj.Save(),
                    });
                }
                frameData.Add(list); 
            }
        }

        public void RewindTo(int frameCount)
        {
            if (frameCount >= 0 && frameCount < frameData.Count)
                StartCoroutine(_RewindTo(frameCount));
        }

        IEnumerator _RewindTo(int frameCount)
        {
            yield return null;

            List<FrameData> list = frameData[frameCount];
            foreach (var fd in list)
            {
                if (!(fd.frameObj is InputSampler))
                    fd.frameObj.Load(fd.frameData);
            }

            AddToHistory();
            Game.instance.inputSampler.simulateProvider.Set(InputParameters(frameCount));
            frameData.RemoveRange(frameCount, frameData.Count - frameCount);
        }

        void AddToHistory()
        {
            List<List<FrameData>> copy = new List<List<FrameData>>(frameData.Count);
            for (int i = 0; i < frameData.Count; ++i)
            {
                List<FrameData> list = new List<FrameData>(frameData[i].Count);
                foreach (var fd in frameData[i])
                {
                    var cfd = new FrameData()
                    {
                        frameObj = fd.frameObj,
                        frameData = fd.frameData.Clone(),
                    };
                    list.Add(cfd);
                }
                copy.Add(list);
            }
            histories.Insert(0, copy);
        }

        IEnumerable<InputParameters> InputParameters(int startFrameCount)
        {
            for (int i = startFrameCount; i < frameData.Count; ++i)
            {
                List<FrameData> list = frameData[i];
                bool nullFrame = true;
                foreach (var fd in list)
                {
                    if (fd.frameData is InputParameters)
                    {
                        yield return (InputParameters)fd.frameData;
                        nullFrame = false;
                        break;
                    }
                }

                if (nullFrame)
                    yield return null;
            }
        }
    }
}
