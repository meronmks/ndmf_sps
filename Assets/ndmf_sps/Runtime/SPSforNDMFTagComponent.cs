using System;
using nadena.dev.modular_avatar.core;
using UnityEngine;

namespace com.meronmks.spsforndmf.runtime
{
    public abstract class SPSforNDMFTagComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        [NonSerialized] public bool forceActive = false;
        public HumanBodyBones boneReference;
        public BoneProxyAttachmentMode attachmentMode;
        public bool enableDepthAnimations = false;
        void Start(){}
    }
}