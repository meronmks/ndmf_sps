﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    public abstract class SPSforNDMFTagComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        [NonSerialized] public bool forceActive = false;
        public bool enableDeformation = true;
        public bool enableDepthAnimations = false;
        public List<DepthAction> depthActions = new List<DepthAction>();
        public bool useHipAvoidance = true;
        public bool detectLength;
        public float length;
        public bool detectRadius;
        public float radius;
        public bool unitsInMeters = true;
        void Start(){}
    }
}