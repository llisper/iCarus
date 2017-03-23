using UnityEngine;
using System;
using Lidgren.Network;

namespace PacMan
{
    public class Player : ISimulate, INetSync, IDisposable
    {
        public int id { get; private set; }
        public PlayerView view { get; private set; }
        public NetConnection connection { get; private set; }

        Vector3 mVelocity = Vector3.zero;

        public Player(int id, PlayerView view, NetConnection connection)
        {
            this.id = id;
            this.view = view;
            this.view.player = this;
            this.connection = connection;
            view.controller.enableOverlapRecovery = false;
        }

        public void SimulateFixedUpdate()
        {
            Vector3 dir = new Vector3(
                InputSampler.Instance.horz,
                0f,
                InputSampler.Instance.vert);
            dir.Normalize();

            float t = Time.deltaTime;
            Vector3 v = mVelocity;
            Vector3 a = view.force * dir + view.friction * (-v).normalized;
            Vector3 at = a * t;
            Vector3 d = (v + 0.5f * at) * t;

            Vector3 prev = view.transform.position;
            view.controller.Move(d);
            Vector3 after = view.transform.position;
            after.y = 0f;
            view.transform.position = after;
            mVelocity = (after - prev) / t;
        }

        public void Dispose()
        {
            GameObject.Destroy(view.gameObject);
        }
    }
}
