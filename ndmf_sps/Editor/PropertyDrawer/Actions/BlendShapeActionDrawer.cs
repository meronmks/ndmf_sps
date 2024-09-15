using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    [CustomPropertyDrawer(typeof(BlendShapeAction))]
    public class BlendShapeActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = EditorStyles.label;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, label, style);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel++;
            var pAllRenderers = property.FindPropertyRelative(nameof(BlendShapeAction.allRenderers));
            EditorGUI.PropertyField(position, pAllRenderers, Localization.G($"{typeof(BlendShapeAction)}.{nameof(BlendShapeAction.allRenderers)}"), true);
            
            EditorGUI.BeginDisabledGroup(pAllRenderers.boolValue);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pRenderer = property.FindPropertyRelative(nameof(BlendShapeAction.renderer));
            EditorGUI.PropertyField(position, pRenderer, Localization.G($"{typeof(BlendShapeAction)}.{nameof(BlendShapeAction.renderer)}"), true);
            EditorGUI.EndDisabledGroup();
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pBlendShape = property.FindPropertyRelative(nameof(BlendShapeAction.blendShape));
            EditorGUI.PropertyField(position, pBlendShape, Localization.G($"{typeof(BlendShapeAction)}.{nameof(BlendShapeAction.blendShape)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pBlendShapeValue = property.FindPropertyRelative(nameof(BlendShapeAction.blendShapeValue));
            EditorGUI.PropertyField(position, pBlendShapeValue, Localization.G($"{typeof(BlendShapeAction)}.{nameof(BlendShapeAction.blendShapeValue)}"), true);

            EditorGUI.indentLevel--;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerUtility.GetPropertyHeight(property, label);
        }
    }
}