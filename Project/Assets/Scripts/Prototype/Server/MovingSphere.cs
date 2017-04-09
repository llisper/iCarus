using UnityEngine;
using Protocol;
using FlatBuffers;

namespace Prototype
{
    public class MovingSphere : MonoBehaviour, ITickObject
    {
        public int id { get { return 0; } }
        public TickObject type { get { return TickObject.MovingSphere; } }
        public TickEvent eventType { get { return TickEvent.MovingSphereEvent; } }

        public float speed = 1f;
        public float angularSpeed = 1f;

        float mRadian;
        float mDistToOrigin;
        Color[] mColorChangeEvents;
        float mColorTimer = 0f;

        static Color[] colorSet = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.blue,
            Color.green,
            Color.cyan,
        };

        void Awake()
        {
            mDistToOrigin = transform.position.magnitude;
            mColorChangeEvents = new Color[TServer.Instance.snapshotOverTick];
        }

        public void SimulateFixedUpdate()
        {
            mRadian += Time.deltaTime * speed;
            transform.position = new Vector3(
                Mathf.Sin(mRadian) * mDistToOrigin,
                transform.position.y,
                Mathf.Cos(mRadian) * mDistToOrigin);

            transform.Rotate(Vector3.up, angularSpeed * Time.deltaTime * Mathf.Rad2Deg, Space.Self);

            Color color;
            if ((mColorTimer += Time.deltaTime) >= 1f)
            {
                color = colorSet[UnityEngine.Random.Range(0, colorSet.Length)];
                mColorTimer = 0f;
            }
            else
            {
                color = Color.black;
            }

            mColorChangeEvents[TServer.Instance.tickCount % mColorChangeEvents.Length] = color;
            if (color != Color.black)
                GetComponent<Renderer>().material.color = color;
        }

        public int Snapshot(FlatBufferBuilder fbb, bool full)
        {
            Protocol.MovingSphere.StartMovingSphere(fbb);
            Vector3 vec3 = transform.position;
            Protocol.MovingSphere.AddPos(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
            vec3 = transform.rotation.eulerAngles;
            Protocol.MovingSphere.AddRot(fbb, Vec3.CreateVec3(fbb, vec3.x, vec3.y, vec3.z));
            return Protocol.MovingSphere.EndMovingSphere(fbb).Value;
        }

        public int SnapshotEvent(FlatBufferBuilder fbb, uint tickCount)
        {
            Color color = mColorChangeEvents[tickCount % mColorChangeEvents.Length];
            if (color != Color.black)
            {
                Protocol.MovingSphereEvent.StartMovingSphereEvent(fbb);
                int colorValue = ((int)(color.r * 255) << 24) |
                                 ((int)(color.g * 255) << 16) |
                                 ((int)(color.b * 255) << 8) |
                                 (int)(color.a * 255);
                Protocol.MovingSphereEvent.AddColor(fbb, colorValue);
                return Protocol.MovingSphereEvent.EndMovingSphereEvent(fbb).Value;
            }
            else
            {
                return -1;
            }
        }
    }
}
