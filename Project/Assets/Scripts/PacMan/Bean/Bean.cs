using UnityEngine;
using System;

namespace PacMan
{
    public class Bean : INetSync, IDisposable
    {
        public int id { get; private set; }
        public int pos { get; private set; }
        public BeanView view { get; private set; }
        public Action<Bean, Player> onEaten;

        public Bean(int id, int pos, BeanView view)
        {
            this.id = id;
            this.pos = pos;
            this.view = view;
            if (null != view)
                view.physicEvents.onTriggerEnter += OnTriggerEnter;
        }

        public void Dispose()
        {
            if (null != view)
            {
                view.physicEvents.onTriggerEnter -= OnTriggerEnter;
                GameObject.Destroy(view.gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (null != onEaten)
            {
                if (other.CompareTag(Tags.Player))
                    onEaten(this, other.GetComponent<PlayerView>().player);
            }
        }

        BeanView mView;
    }
}
