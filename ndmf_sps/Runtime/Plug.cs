using UnityEngine;

namespace com.meronmks.ndmfsps.runtime
{
    [AddComponentMenu(Values.COMPONENTS_BASE + nameof(Plug))]
    public class Plug : SPSforNDMFTagComponent
    {
        public bool automaticallyFindMesh;
        public bool detectLengthFromMesh;
        public bool detectRadiusFromMesh;
        public bool automaticallyMaskUsingBoneWeights;
    }
}