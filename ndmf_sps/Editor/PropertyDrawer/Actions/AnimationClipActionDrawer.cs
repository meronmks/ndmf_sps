using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.meronmks.ndmfsps
{
    [CustomPropertyDrawer(typeof(AnimationClipAction))]
    public class AnimationClipActionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var style = EditorStyles.label;
            style.alignment = TextAnchor.UpperLeft;
            EditorGUI.LabelField(position, label, style);
            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.indentLevel++;
            var pClip = property.FindPropertyRelative(nameof(AnimationClipAction.clip));
            EditorGUI.PropertyField(position, pClip, Localization.G($"{typeof(AnimationClipAction)}.{nameof(AnimationClipAction.clip)}"), true);
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return PropertyDrawerUtility.GetPropertyHeight(property, label);
        }
    }
}