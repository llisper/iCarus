using UnityEngine;
using iCarus.Singleton;

namespace PacMan
{
    public sealed class Camera : SingletonBehaviour<Camera>
    {
        public float height = 10;
        public float followSpeed = 1f;

        void FixedUpdate()
        {
            transform.position = Vector3.Lerp(
                transform.position,
                TargetPosition(),
                followSpeed * Time.deltaTime);
        }

        Vector3 TargetPosition()
        {
            Vector3 position = Vector3.zero;
            if (null != PlayerManager.Instance)
            {
                Player player = PlayerManager.Instance.localPlayer;
                if (null != player && null != player.view)
                    position = player.view.transform.position;
                position.y += height;
            }
            return position;
        }
    }
}
