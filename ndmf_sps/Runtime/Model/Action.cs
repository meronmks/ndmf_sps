using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    public enum ActionType
    {
        AnimationClip,
        BlendShape,
        ObjectToggle,
        FxFloat
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
        public enum Mode
        {
            TurnOn,
            TurnOff,
            Toggle
        }
        
        public GameObject obj;
        public Mode mode;
    }

    [Serializable]
    public class FxFloatAction : IAction
    {
        public string name;
        public float value = 1f;
    }
}