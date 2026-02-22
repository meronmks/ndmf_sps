using UnityEditor;

namespace com.meronmks.ndmfsps
{
    using runtime;

    [CustomEditor(typeof(Plug))]
    internal class PlugEditor : Editor
    {
        private bool advancedFoldout;
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            Plug plug = target as Plug;
            Localization.SelectLanguageGUI();
            CommonGUI.ShowCommonHelpBox();
            EditorGUILayout.HelpBox(Localization.S("inspector.common.plugWarnMes"), MessageType.Warning);
            EditorGUILayout.Separator();

            var pAutomaticallyFindMesh = serializedObject.FindProperty(nameof(Plug.automaticallyFindMesh));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pAutomaticallyFindMesh,
                Localization.G("inspector.plug.automaticallyFindMesh"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (!pAutomaticallyFindMesh.boolValue)
            {
                EditorGUI.indentLevel++;
                var pMeshRenderers = serializedObject.FindProperty(nameof(Plug.meshRenderers));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pMeshRenderers, Localization.G("inspector.plug.meshRenderers"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
            
            var pDetectTransform4Mesh = serializedObject.FindProperty(nameof(Plug.detectTransform4Mesh));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pDetectTransform4Mesh,
                Localization.G("inspector.plug.detectTransform4Mesh"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            var pDetectLength = serializedObject.FindProperty(nameof(Plug.detectLength));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pDetectLength, Localization.G("inspector.plug.detectLength"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (!pDetectLength.boolValue)
            {
                EditorGUI.indentLevel++;
                var pLength = serializedObject.FindProperty(nameof(Plug.length));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pLength, Localization.G("inspector.common.length"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
            
            var pDetectRadius = serializedObject.FindProperty(nameof(Plug.detectRadius));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pDetectRadius, Localization.G("inspector.plug.detectRadius"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (!pDetectRadius.boolValue)
            {
                EditorGUI.indentLevel++;
                var pRadius = serializedObject.FindProperty(nameof(Plug.radius));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pRadius, Localization.G("inspector.common.radius"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }

            var pAutomaticallyMaskUsingBoneWeights = serializedObject.FindProperty(nameof(Plug.automaticallyMaskUsingBoneWeights));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pAutomaticallyMaskUsingBoneWeights, Localization.G("inspector.plug.automaticallyMaskUsingBoneWeights"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            var pTextureMask = serializedObject.FindProperty(nameof(Plug.textureMask));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pTextureMask, Localization.G("inspector.plug.textureMask"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            var pEnableDeformation = serializedObject.FindProperty(nameof(Plug.enableDeformation));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pEnableDeformation, Localization.G("inspector.common.enableDeformation"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (pEnableDeformation.boolValue)
            {
                EditorGUI.indentLevel++;
                var pAutoRig = serializedObject.FindProperty(nameof(Plug.autoRig));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pAutoRig, Localization.G("inspector.plug.autoRig"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                
                var pPostBakeActions = serializedObject.FindProperty(nameof(Plug.postBakeActions));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pPostBakeActions, Localization.G("inspector.plug.postBakeActions"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                
                var pAnimatedToggle = serializedObject.FindProperty(nameof(Plug.animatedToggle));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pAnimatedToggle, Localization.G("inspector.plug.animatedToggle"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                
                var pAnimatedBlendshapes = serializedObject.FindProperty(nameof(Plug.animatedBlendshapes));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pAnimatedBlendshapes, Localization.G("inspector.plug.animatedBlendshapes"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                
                var pAllowHoleOverrun = serializedObject.FindProperty(nameof(Plug.allowHoleOverrun));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pAllowHoleOverrun, Localization.G("inspector.plug.allowHoleOverrun"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
            }
            
            var pEnableDepthAnimations = serializedObject.FindProperty(nameof(Plug.enableDepthAnimations));
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pEnableDepthAnimations, Localization.G("inspector.common.enableDepthAnimations"));
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (pEnableDepthAnimations.boolValue)
            {
                EditorGUI.indentLevel++;
                var pDepthActions = serializedObject.FindProperty(nameof(Plug.depthActions));
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(pDepthActions, Localization.G("inspector.common.depthActions"));
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                }
                EditorGUI.indentLevel--;
                EditorGUILayout.Separator();
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
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}