using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    public static class PropertyDrawerUtility
    {
        public static float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            property = property.serializedObject.FindProperty(property.propertyPath);
            var height = 0.0f;
        
            // プロパティ名
            height += EditorGUIUtility.singleLineHeight;
            height += EditorGUIUtility.standardVerticalSpacing;

            if (!property.hasChildren) {
                // 子要素が無ければラベルだけ表示
                return height;
            }
        
            if (property.isExpanded) {
        
                // 最初の要素
                property.NextVisible(true);
                var depth = property.depth;
                height += EditorGUI.GetPropertyHeight(property, true);
                height += EditorGUIUtility.standardVerticalSpacing;
            
                // それ以降の要素
                while(property.NextVisible(false))
                {
                    // depthが最初の要素と同じもののみ処理
                    if (property.depth != depth) {
                        break;
                    }
                    height += EditorGUI.GetPropertyHeight(property, true);
                    height += EditorGUIUtility.standardVerticalSpacing;
                }
                // 最後はスペース不要なので削除
                height -= EditorGUIUtility.standardVerticalSpacing;
            }

            return height;
        }
    }
}