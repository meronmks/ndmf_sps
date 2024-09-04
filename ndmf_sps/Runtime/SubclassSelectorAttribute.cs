using System;
using UnityEngine;

/***
 * Original: https://github.com/baba-s/Unity-SerializeReferenceExtensions
 */

namespace com.meronmks.ndmfsps.runtime
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
        Type m_type;

        public SubclassSelectorAttribute(System.Type type)
        {
            m_type = type;
        }

        public Type GetFieldType()
        {
            return m_type;
        }
    }
}