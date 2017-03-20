using UnityEngine;

namespace Experimental
{
    public class InputSampler : MonoBehaviour, IFrame
    {
        public InputParameters parameters = new InputParameters();
        public DeviceProvider deviceProvider = new DeviceProvider();
        public SimulateProvider simulateProvider = new SimulateProvider();

        public string identity { get { return "InputSampler"; } }

        void Awake()
        {
            Game.instance.frameRecorder.Subscribe(this);
        }

        void FixedUpdate()
        {
            if (simulateProvider.hasData)
                simulateProvider.Get(parameters);
            else
                deviceProvider.Get(parameters);
        }

        public IFrameData Save()
        {
            return parameters.Clone();
        }

        public void Load(IFrameData frameData)
        {
            throw new System.NotImplementedException();
        }
    }
}
