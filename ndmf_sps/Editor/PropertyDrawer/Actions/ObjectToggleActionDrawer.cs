using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    [CustomPropertyDrawer(typeof(ObjectToggleAction))]
    public class ObjectToggleActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = EditorStyles.label;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, label, style);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel++;
            var pObj = property.FindPropertyRelative(nameof(ObjectToggleAction.obj));
            EditorGUI.PropertyField(position, pObj, Localization.G($"{typeof(ObjectToggleAction)}.{nameof(ObjectToggleAction.obj)}"), true);
            
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            var pMode = property.FindPropertyRelative(nameof(ObjectToggleAction.mode));
            EditorGUI.PropertyField(position, pMode, Localization.G($"{typeof(ObjectToggleAction)}.{nameof(ObjectToggleAction.mode)}"), true);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerUtility.GetPropertyHeight(property, label);
        }
    }
}