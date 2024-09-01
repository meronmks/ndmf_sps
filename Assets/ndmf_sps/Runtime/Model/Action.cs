using System;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [Serializable]
    public class Action
    {
        
    }

    [Serializable]
    public class BlendShapeAction : Action
    {
        public string blendShape;
        public float blendShapeValue = 100;
        public Renderer renderer;
        public bool allRenderers = true;
    }
}