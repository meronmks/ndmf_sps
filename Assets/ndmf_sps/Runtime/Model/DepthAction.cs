using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [Serializable]
    public class DepthAction
    {
        [SerializeReference, SubclassSelector(typeof(IAction))] public List<IAction> actions = new ();
        public float startDistance = 0;
        public float endDistance = -0.25f;
        public bool enableSelf;
        public float smoothingSeconds = 0.25f;
    }
}