using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    [CustomPropertyDrawer(typeof(FxFloatAction))]
    public class FxFloatActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = EditorStyles.label;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, label, style);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel++;
            var pName = property.FindPropertyRelative(nameof(FxFloatAction.name));
            EditorGUI.PropertyField(position, pName, Localization.G($"{typeof(FxFloatAction)}.{nameof(FxFloatAction.name)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pValue = property.FindPropertyRelative(nameof(FxFloatAction.value));
            EditorGUI.PropertyField(position, pValue, Localization.G($"{typeof(FxFloatAction)}.{nameof(FxFloatAction.value)}"), true);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerUtility.GetPropertyHeight(property, label);
        }
    }
}