using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    public enum ActionType
    {
        AnimationClip,
        BlendShape,
        ObjectToggle
    }
    
    public interface IAction
    {
    }

    [Serializable]
    public class AnimationClipAction : IAction
    {
        public AnimationClip clip;
    }

    [Serializable]
    public class BlendShapeAction : IAction
    {
        public string blendShape;
        public float blendShapeValue = 100;
        public Renderer renderer;
        public bool allRenderers = true;
    }

    [Serializable]
    public class ObjectToggleAction : IAction
    {
        public GameObject obj;
        public bool mode;
    }
}