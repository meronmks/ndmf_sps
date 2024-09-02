using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;

namespace com.meronmks.ndmfsps
{
    using runtime;
    
    [CustomEditor(typeof(Socket))]
    internal class SocketEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Socket socket = target as Socket;
            Localization.SelectLanguageGUI();
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField(Localization.S("inspector.common.boneReference"));
            socket.boneReference = (HumanBodyBones)EditorGUILayout.EnumPopup(socket.boneReference);
            EditorGUILayout.LabelField(Localization.S("inspector.common.attachmentMode"));
            socket.attachmentMode = (BoneProxyAttachmentMode)EditorGUILayout.EnumPopup(socket.attachmentMode);
        }
    }
}