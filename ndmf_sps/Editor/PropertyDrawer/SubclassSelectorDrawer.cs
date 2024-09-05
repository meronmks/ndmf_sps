using System;
using System.Linq;
using System.Reflection;
using com.meronmks.ndmfsps.runtime;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

/***
 * Original: https://github.com/baba-s/Unity-SerializeReferenceExtensions
 */

namespace com.meronmks.ndmfsps
{
	[CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
	public class SubclassSelectorDrawer : PropertyDrawer
	{
	    bool initialized = false;
	    Type[] inheritedTypes;
	    string[] typePopupNameArray;
	    string[] typeFullNameArray;
	    int currentTypeIndex;

	    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	    {
	        if (property.propertyType != SerializedPropertyType.ManagedReference) return;
	        if(!initialized) {
	            Initialize(property);
	            GetCurrentTypeIndex(property.managedReferenceFullTypename);
	            initialized = true;
	        }
	        int selectedTypeIndex = EditorGUI.Popup(GetPopupPosition(position), currentTypeIndex, typePopupNameArray);
	        UpdatePropertyToSelectedTypeIndex(property, selectedTypeIndex);
	        EditorGUI.PropertyField(position, property, label, true);
	    }

	    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	    {
	        return EditorGUI.GetPropertyHeight(property, true);
	    }

	    private void Initialize(SerializedProperty property)
	    {
	        SubclassSelectorAttribute utility = (SubclassSelectorAttribute)attribute;
	        // 元実装の自動取得だと稀に壊れるパターンがあるのであえて型を指定する方法で回避
	        GetAllInheritedTypes(utility.GetFieldType(), false);
	        GetInheritedTypeNameArrays();
	    }

	    private void GetCurrentTypeIndex(string typeFullName)
	    {
	        currentTypeIndex = Array.IndexOf(typeFullNameArray, typeFullName);
	    }

	    void GetAllInheritedTypes(Type baseType, bool includeMono)
	    {
	        Type monoType = typeof(MonoBehaviour);
	        inheritedTypes = AppDomain.CurrentDomain.GetAssemblies()
	            .SelectMany(s => s.GetTypes())
	            .Where(p => baseType.IsAssignableFrom(p) && p.IsClass && (!monoType.IsAssignableFrom(p) || includeMono))
	            .Prepend(null)
	            .ToArray();
	    }

	    private void GetInheritedTypeNameArrays()
	    {
	        typePopupNameArray = inheritedTypes.Select(type => type == null ? Localization.S("inspector.action.none") : Localization.S(type.ToString())).ToArray();
	        typeFullNameArray = inheritedTypes.Select(type => type == null ? "None" : string.Format("{0} {1}", type.Assembly.ToString().Split(',')[0], type.FullName)).ToArray();
	    }

	    public void UpdatePropertyToSelectedTypeIndex(SerializedProperty property, int selectedTypeIndex)
	    {
	        if (currentTypeIndex == selectedTypeIndex) return;
	        currentTypeIndex = selectedTypeIndex;
	        Type selectedType = inheritedTypes[selectedTypeIndex];
	        property.managedReferenceValue =
	            selectedType == null ? null : Activator.CreateInstance(selectedType);
	    }

	    Rect GetPopupPosition(Rect currentPosition)
	    {
	        Rect popupPosition = new Rect(currentPosition);
	        popupPosition.width -= EditorGUIUtility.labelWidth;
	        popupPosition.x += EditorGUIUtility.labelWidth;
	        popupPosition.height = EditorGUIUtility.singleLineHeight;
	        return popupPosition;
	    }
	}
}