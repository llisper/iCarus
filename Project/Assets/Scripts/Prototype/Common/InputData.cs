using UnityEngine;

namespace Prototype.Common
{
    public struct InputData
    {
        public uint index;          // sample index
        public byte keyboard;       // W-A-S-D
        public bool mouseHasHit;
        public Vector3 mouseHit;    // X-Z position on Ground hit by mouse

        public bool valid { get { return index > 0; } }
    }
}
