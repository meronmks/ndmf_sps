using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using Object = UnityEngine.Object;

namespace com.meronmks.ndmfsps
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class Processor
    {
        private static SPSforNDMFTagComponent[] components;
        private static Socket[] sockets;
        private static Plug[] plugs;

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
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            
            sockets = ctx.AvatarRootObject.GetComponentsInChildren<Socket>(true);
            foreach (var socket in sockets)
            {
                SocketProcessor.CreateSender(animator, socket.transform, socket);
                SocketProcessor.CreateLights(socket.transform, socket.mode);
                SocketProcessor.CreateHaptics(ctx, animator, socket.transform, socket);
                if (socket.enableDepthAnimations)
                {
                    SocketProcessor.CreateAnimations(ctx, socket.transform, socket.depthActions);
                }
                SocketProcessor.CreateAutoDistance(ctx, socket.transform);
            }
            
            plugs = ctx.AvatarRootObject.GetComponentsInChildren<Plug>(true);

            foreach (var plug in plugs)
            {
                var size = PlugProcessor.GetWorldSize(plug);
                var bakedSpsPlug = PlugProcessor.CreateBakedSpsPlug(plug);
                bakedSpsPlug.transform.localPosition = size.localPosition;
                bakedSpsPlug.transform.localRotation = size.localRotation;

                PlugProcessor.CreateSenders(bakedSpsPlug, plug, size, animator);
                PlugProcessor.CreateHaptic(bakedSpsPlug, plug, size, animator);
                PlugProcessor.CreateSpsPlus(bakedSpsPlug, plug, size, animator);

                foreach (var renderer in size.renderers)
                {
                    try
                    {
                        var skin = PlugProcessor.CreateNormalizeRenderer(renderer, bakedSpsPlug, size.worldLength);
                    }
                    catch (Exception e)
                    {
                        NDMFConsole.LogError("ndmf.console.plug.failedToConfigureRenderer", e);
                    }
                }
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
            float radius, 
            Vector3 pos, 
            Quaternion rot, 
            bool localOnly, 
            string[] collisionTags, 
            ContactReceiver.ReceiverType receiverType, 
            string parameter,
            Animator animator,
            ReceiverParty party,
            float height = 0,
            bool worldScale = true,
            bool useHipAvoidance = true)
        {
            var receiver = target.AddComponent<VRCContactReceiver>();
            receiver.shapeType = ContactBase.ShapeType.Sphere;
            receiver.radius = radius;
            receiver.position = pos;
            receiver.allowSelf = party == ReceiverParty.Others;
            receiver.allowOthers = party == ReceiverParty.Self;
            receiver.localOnly = localOnly;
            receiver.receiverType = receiverType;
            // この名前にあったアニメーションパラメータが必要
            receiver.parameter = parameter;
            
            if (height > 0) {
                receiver.shapeType = ContactBase.ShapeType.Capsule;
                receiver.height = height;
                receiver.rotation = rot;
            }

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
            float radius, 
            Vector3 pos, 
            Quaternion rot, 
            string[] collisionTags,
            Animator animator,
            bool worldScale = true,
            bool useHipAvoidance = true,
            float height = 0)
        {
            var sender = target.AddComponent<VRCContactSender>();
            
            sender.shapeType = ContactBase.ShapeType.Sphere;
            sender.radius = radius;
            sender.position = pos;
            sender.rotation = rot;
            
            if (height > 0)
            {
                sender.shapeType = ContactBase.ShapeType.Capsule;
                sender.height = height;
                sender.rotation = rot;
            }

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

        internal static Transform GetMeshRoot(Renderer r)
        {
            if (r is SkinnedMeshRenderer skin && skin.rootBone != null)
            {
                return skin.rootBone;
            }

            return r.transform;
        }

        internal static Quaternion? GetMaterialDpsRotation(Renderer r)
        {
            return r.sharedMaterials.Select(GetMaterialDpsRotation).FirstOrDefault(c => c != null);
        }

        internal static Quaternion? GetMaterialDpsRotation(Material mat)
        {
            if (IsDps(mat))
            {
                return Quaternion.identity;
            }

            if (IsTps(mat))
            {
                return GetTpsRotation(mat);
            }

            return null;
        }

        internal static bool IsDps(Material mat)
        {
            if (mat == null) return false;
            if (!mat.shader) return false;
            if (mat.shader.name == "Raliv/Penetrator") return true;
            if (mat.shader.name.Contains("XSToon") && mat.shader.name.Contains("Penetrator")) return true;
            if (mat.HasProperty("_PenetratorEnabled") && mat.GetFloat("_PenetratorEnabled") > 0) return true;
            if (mat.shader.name.Contains("DPS") && mat.HasProperty("_ReCurvature")) return true;
            return false;
        }

        internal static bool IsTps(Material mat)
        {
            if (mat == null) return false;
            return mat.HasProperty("_TPSPenetratorEnabled") && mat.GetFloat("_TPSPenetratorEnabled") > 0;
        }

        internal static Quaternion GetTpsRotation(Material mat)
        {
            if (mat.HasProperty("_TPS_PenetratorForward")) {
                var c = mat.GetVector("_TPS_PenetratorForward");
                return Quaternion.LookRotation(new Vector3(c.x, c.y, c.z));
            }
            return Quaternion.identity;
        }
    }
}