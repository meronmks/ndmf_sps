using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    [CustomPropertyDrawer(typeof(DepthAction))]
    public class DepthActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = EditorStyles.label;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, label, style);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pActions = property.FindPropertyRelative(nameof(DepthAction.actions));
            EditorGUI.PropertyField(position, pActions, Localization.G($"{typeof(DepthAction)}.{nameof(DepthAction.actions)}"), true);
            
            position.y += EditorGUI.GetPropertyHeight(pActions, true) + EditorGUIUtility.standardVerticalSpacing;
            var pStartDistance = property.FindPropertyRelative(nameof(DepthAction.startDistance));
            EditorGUI.PropertyField(position, pStartDistance, Localization.G($"{typeof(DepthAction)}.{nameof(DepthAction.startDistance)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pEndDistance = property.FindPropertyRelative(nameof(DepthAction.endDistance));
            EditorGUI.PropertyField(position, pEndDistance, Localization.G($"{typeof(DepthAction)}.{nameof(DepthAction.endDistance)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pEnableSelf = property.FindPropertyRelative(nameof(DepthAction.enableSelf));
            EditorGUI.PropertyField(position, pEnableSelf, Localization.G($"{typeof(DepthAction)}.{nameof(DepthAction.enableSelf)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pSmoothingSeconds = property.FindPropertyRelative(nameof(DepthAction.smoothingSeconds));
            EditorGUI.PropertyField(position, pSmoothingSeconds, Localization.G($"{typeof(DepthAction)}.{nameof(DepthAction.smoothingSeconds)}"), true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property.isExpanded = true;
            return PropertyDrawerUtility.GetPropertyHeight(property, label);
        }
    }
}