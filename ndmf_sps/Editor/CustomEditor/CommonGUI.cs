using UnityEditor;

namespace com.meronmks.ndmfsps
{
    internal static class CommonGUI
    {
        internal static void ShowCommonHelpBox()
        {
            EditorGUILayout.HelpBox(Localization.S("inspector.common.commonInfoMes"), MessageType.Info);
        }
    }
}