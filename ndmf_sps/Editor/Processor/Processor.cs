using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace com.meronmks.ndmfsps
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class Processor
    {
        private static SPSforNDMFTagComponent[] components;
        private static Socket[] sockets;

        internal static readonly string[] selfContacts =
        {
            "Hand",
            "Finger",
            "Foot"
        };

        internal static readonly string[] bodyContacts =
        {
            "Head",
            "Hand",
            "Foot",
            "Finger"
        };
        
        internal enum ReceiverParty
        {
            Self,
            Others,
            Both
        }
        
        internal static void CreateComponent(BuildContext ctx)
        {
            sockets = ctx.AvatarRootObject.GetComponentsInChildren<Socket>(true);

            foreach (var socket in sockets)
            {
                SocketProcessor.CreateSender(ctx, socket.transform, socket);
                SocketProcessor.CreateLights(socket.transform, socket.mode);
                SocketProcessor.CreateHaptics(ctx, socket.transform, socket);
                if (socket.enableDepthAnimations)
                {
                    SocketProcessor.CreateAnimations(ctx, socket.transform, socket.depthActions);
                }
                SocketProcessor.CreateAutoDistance(ctx, socket.transform);
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

        internal static void CreateVRCContactReceiver(
            GameObject target, 
            ContactBase.ShapeType shapeType, 
            float radius, 
            float height, 
            Vector3 pos, 
            Quaternion rot, 
            bool allowSelf, 
            bool allowOthers, 
            bool localOnly, 
            string[] collisionTags, 
            ContactReceiver.ReceiverType receiverType, 
            string parameter,
            Animator animator,
            ReceiverParty party,
            bool worldScale = true,
            bool useHipAvoidance = true)
        {
            var receiver = target.AddComponent<VRCContactReceiver>();
            receiver.shapeType = shapeType;
            receiver.radius = radius;
            receiver.height = height;
            receiver.position = pos;
            receiver.rotation = rot;
            receiver.allowSelf = allowSelf;
            receiver.allowOthers = allowOthers;
            receiver.localOnly = localOnly;
            receiver.receiverType = receiverType;
            // この名前にあったアニメーションパラメータが必要
            receiver.parameter = parameter;

            if (worldScale)
            {
                receiver.position /= target.transform.lossyScale.x;
                receiver.radius /= target.transform.lossyScale.x;
                receiver.height /= target.transform.lossyScale.x;
            }
            
            AddTags(receiver, "", collisionTags);
            
            if (animator == null || !animator.isHuman) return;
            var bone = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (bone == null) return;
            if (party == ReceiverParty.Self && target == bone.gameObject && useHipAvoidance)
            {
                AddTags(receiver, "_SelfNotOnHips", collisionTags);
            }
        }

        internal static void CreateVRCContactSender(
            GameObject target, 
            ContactBase.ShapeType shapeType, 
            float radius, 
            Vector3 pos, 
            Quaternion rot, 
            string[] collisionTags,
            Animator animator,
            bool worldScale = true,
            bool useHipAvoidance = true)
        {
            var sender = target.AddComponent<VRCContactSender>();
            
            sender.shapeType = shapeType;
            sender.radius = radius;
            sender.position = pos;
            sender.rotation = rot;

            if (worldScale)
            {
                sender.position /= target.transform.lossyScale.x;
                sender.radius /= target.transform.lossyScale.x;
                sender.height /= target.transform.lossyScale.x;
            }

            AddTags(sender, "", collisionTags);

            if (animator == null || !animator.isHuman) return;
            var bone = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (bone == null) return;
            if (target == bone.gameObject || !useHipAvoidance)
            {
                AddTags(sender, "_SelfNotOnHips", collisionTags);
            }
        }

        private static void AddTags(VRCContactSender sender,string suffix, string[] tags)
        {
            sender.collisionTags.AddRange(CreateTags(tags, suffix));
        }

        private static void AddTags(VRCContactReceiver receiver, string suffix, string[] tags)
        {
            receiver.collisionTags.AddRange(CreateTags(tags, suffix));
        }

        private static List<string> CreateTags(string[] tags, params string[] suffix)
        {
            return tags.SelectMany(tag =>
            {
                if (!tag.StartsWith("SPSLL_") && !tag.StartsWith("SPS_") && !tag.StartsWith("TPS_"))
                {
                    return new[] { tag };
                }

                return suffix.Select(s => tag + s);
            }).ToList();
        }
    }
}