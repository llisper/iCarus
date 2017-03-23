using System.Collections.Generic;
using iCarus.Singleton;

namespace PacMan
{
    public interface ISimulate
    {
        void SimulateFixedUpdate();
    }

    public sealed class SimulateManager : SingletonBehaviour<SimulateManager>
    {
        public int ticks { get { return mTicks; } }

        public void SimulateStart()
        {
            mRunning = true;
        }

        public void SimulateStop()
        {
            mRunning = false;
        }

        public void Add(ISimulate obj)
        {
            if (!mSimulateObjects.Contains(obj))
                mSimulateObjects.Add(obj);
        }

        public void Remove(ISimulate obj)
        {
            mSimulateObjects.Remove(obj);
        }

        public override void DestroySingleton()
        {
            mSimulateObjects.Clear();
            mTemporary.Clear();
            base.DestroySingleton();
        }

        int mTicks = 0;
        bool mRunning = false;
        List<ISimulate> mSimulateObjects = new List<ISimulate>();
        List<ISimulate> mTemporary = new List<ISimulate>();

        void FixedUpdate()
        {
            if (mRunning)
            {
                ++mTicks;
                mTemporary.AddRange(mSimulateObjects);
                foreach (ISimulate obj in mTemporary)
                    obj.SimulateFixedUpdate();
                mTemporary.Clear();
            }
        }
    }
}
