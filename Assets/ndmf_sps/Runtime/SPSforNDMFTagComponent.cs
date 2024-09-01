using System;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    public abstract class SPSforNDMFTagComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        [NonSerialized] public bool forceActive = false;
        public HumanBodyBones boneReference;
        public BoneProxyAttachmentMode attachmentMode;
        public bool enableDepthAnimations = false;
        public List<DepthAction> depthActions = new List<DepthAction>();
        void Start(){}
    }
}