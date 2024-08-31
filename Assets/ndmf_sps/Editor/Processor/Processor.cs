using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace com.meronmks.spsforndmf
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class Processor
    {
        private static SPSforNDMFTagComponent[] components;
        private static Socket[] sockets;
        
        internal static void CreateComponent(BuildContext ctx)
        {
            sockets = ctx.AvatarRootObject.GetComponentsInChildren<Socket>(true);

            foreach (var socket in sockets)
            {
                var boneProxy = socket.gameObject.GetComponent<ModularAvatarBoneProxy>();
                if (!boneProxy)
                {
                    boneProxy = socket.gameObject.AddComponent<ModularAvatarBoneProxy>();
                    boneProxy.boneReference = socket.boneReference;
                    boneProxy.attachmentMode = socket.attachmentMode;
                }

                SocketProcessor.CreateSender(socket.transform);
                SocketProcessor.CreateLights(socket.transform, socket.mode);
                SocketProcessor.CreateHaptics(socket.transform);
                if (socket.enableDepthAnimations)
                {
                    SocketProcessor.CreateAnimations(socket.transform);
                }
                SocketProcessor.CreateAutoDistance(socket.transform);
            }
        }

        internal static void RemoveComponent(BuildContext ctx)
        {
            foreach (var component in ctx.AvatarRootObject.GetComponentsInChildren<SPSforNDMFTagComponent>(true))
            {
                Object.DestroyImmediate(component);
            }
        }

        internal static GameObject CreateParentGameObject(string name, Transform root)
        {
            var parent = new GameObject(name);
            parent.transform.SetParent(root);
            ResetTransform(parent.transform);
            return parent;
        }

        internal static void ResetTransform(Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}