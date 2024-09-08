using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [AddComponentMenu(Values.COMPONENTS_BASE + nameof(Plug))]
    public class Plug : SPSforNDMFTagComponent
    {
        public bool automaticallyFindMesh = true;
        public List<Renderer> meshRenderers = new();
        public bool detectTransform4Mesh = true;
        public bool automaticallyMaskUsingBoneWeights = true;
        public Texture2D textureMask = null;
        public bool autoRig = true;
        [SerializeReference, SubclassSelector(typeof(IAction))] public List<IAction> postBakeActions = new ();
        public bool animatedToggle = false;
        public List<string> animatedBlendshapes = new();
        public bool allowHoleOverrun = true;

        private void Reset()
        {
            detectLength = true;
            detectRadius = true;
        }
    }
}