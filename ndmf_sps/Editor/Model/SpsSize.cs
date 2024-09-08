using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    internal class SpsSize
    {
        public ICollection<Renderer> renderers;
        public float worldLength;
        public float worldRadius;
        public Vector3 localPosition;
        public Quaternion localRotation;
        public MultiMapHashSet<GameObject, int> matSlots;
    }
}