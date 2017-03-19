using System.Collections.Generic;

namespace Experimental
{
    public class SimulateProvider : IInputProvider
    {
        int mIndex = 0;
        List<InputParameters> mSource = new List<InputParameters>();

        public bool hasData { get { return mIndex < mSource.Count; } }

        public void Get(InputParameters parameters)
        {
            if (mIndex < mSource.Count)
                mSource[mIndex++].Overwrite(parameters);
        }

        public void Set(IEnumerable<InputParameters> parameters)
        {
            mIndex = 0;
            mSource.Clear();
            mSource.AddRange(parameters);
        }
    }
}
