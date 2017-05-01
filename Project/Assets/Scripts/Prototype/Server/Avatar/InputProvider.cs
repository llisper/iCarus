using System.Collections.Generic;
using Prototype.Common;

namespace Prototype.Server
{
    public class InputProvider
    {
        uint[] mAckInputs;
        Queue<InputData> mInputQueue;
        InputData mCurrentInput;

        public InputProvider(uint[] ackInputs, Queue<InputData> inputQueue)
        {
            mAckInputs = ackInputs;
            mInputQueue = inputQueue;
        }

        public void MoveNext()
        {
            if (mInputQueue.Count <= 0)
            {
                mAckInputs[SyncManager.Instance.tickCount % mAckInputs.Length] = 0;
                mCurrentInput = default(InputData);
            }
            else
            {
                mCurrentInput = mInputQueue.Dequeue();
                mAckInputs[SyncManager.Instance.tickCount % mAckInputs.Length] = mCurrentInput.index;
            }
        }

        public InputData current { get { return mCurrentInput; } }
    }
}
