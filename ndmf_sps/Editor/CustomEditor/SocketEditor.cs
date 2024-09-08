using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    using runtime;
    
    [CustomEditor(typeof(Socket))]
    internal class SocketEditor : Editor
    {
        private bool advancedFoldout;
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            Socket socket = target as Socket;
            Localization.SelectLanguageGUI();
            CommonGUI.ShowCommonHelpBox();
            EditorGUILayout.Separator();

            var pEnableDeformation = serializedObject.FindProperty(nameof(Socket.enableDeformation));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pEnableDeformation, Localization.G("inspector.common.enableDeformation"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (pEnableDeformation.boolValue)
            {
                EditorGUI.indentLevel++;
                var pMode = serializedObject.FindProperty(nameof(Socket.mode));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMode, Localization.G("inspector.socket.mode"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }

            var pEnableDepthAnimations = serializedObject.FindProperty(nameof(Socket.enableDepthAnimations));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pEnableDepthAnimations, Localization.G("inspector.common.enableDepthAnimations"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (pEnableDepthAnimations.boolValue)
            {
                EditorGUI.indentLevel++;
                var pDepthActions = serializedObject.FindProperty(nameof(Socket.depthActions));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pDepthActions, Localization.G("inspector.common.depthActions"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
            
            var pEnableActiveAnimation = serializedObject.FindProperty(nameof(Socket.enableActiveAnimation));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pEnableActiveAnimation, Localization.G("inspector.socket.enableActiveAnimation"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            if (pEnableActiveAnimation.boolValue)
            {
                EditorGUI.indentLevel++;
                var pActiveAnimationActions = serializedObject.FindProperty(nameof(Socket.activeAnimationActions));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pActiveAnimationActions, Localization.G("inspector.socket.activeAnimationActions"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
            
            var pHaptics = serializedObject.FindProperty(nameof(Socket.haptics));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pHaptics, Localization.G("inspector.socket.haptics"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, Localization.G("inspector.common.advancedFoldout"));
            if (advancedFoldout)
            {
                EditorGUI.indentLevel++;
                var pUseHipAvoidance = serializedObject.FindProperty(nameof(Socket.useHipAvoidance));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pUseHipAvoidance, Localization.G("inspector.common.useHipAvoidance"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                var pUnitsInMeters = serializedObject.FindProperty(nameof(Socket.unitsInMeters));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pUnitsInMeters, Localization.G("inspector.common.unitsInMeters"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}