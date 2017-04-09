using UnityEngine;
using System.Collections;
using Protocol;
using System;

namespace Prototype
{
    public class TPlayerClient : ITickObjectClient
    {
        public int id { get { return TClient.Instance.playerName.GetHashCode(); } }

        public void EventUpdate(TickEventT evt)
        {
            throw new NotImplementedException();
        }

        public void FullUpdate(uint tick, TickObjectBox box)
        {
            throw new NotImplementedException();
        }

        public void Lerping(float t, uint tick, TickObjectBox box)
        {
            throw new NotImplementedException();
        }
    }
}
