using System.Collections.Generic;
using com.meronmks.ndmfsps.runtime;
using UnityEditor;

namespace com.meronmks.ndmfsps
{
    internal static class PropertyDrawerDatabase
    {
        private  static Dictionary<System.Type, PropertyDrawer> _drawers = new Dictionary<System.Type, PropertyDrawer>();

        static PropertyDrawerDatabase()
        {
            _drawers = new Dictionary<System.Type, PropertyDrawer>();
        
            // クラスと対応するPropertyDrawerを登録しておく
            _drawers.Add(typeof(AnimationClipAction), new AnimationClipActionDrawer());
            _drawers.Add(typeof(BlendShapeAction), new BlendShapeActionDrawer());
        }

        internal static PropertyDrawer GetDrawer(System.Type fieldType)
        {
            PropertyDrawer drawer;
            return _drawers.TryGetValue(fieldType, out drawer) ? drawer : null;
        }
    }
}