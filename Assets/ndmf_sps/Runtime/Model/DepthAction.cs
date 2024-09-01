using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [Serializable]
    public class DepthAction
    {
        [SerializeReference] public List<Action> actions = new List<Action>();
        public float startDistance = 0;
        public float endDistance = -0.25f;
        public bool enableSelf;
        public float smoothingSeconds = 0.25f;
    }
}