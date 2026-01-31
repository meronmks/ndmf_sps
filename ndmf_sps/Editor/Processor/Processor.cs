using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDKBase;
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

        internal static void FindSpsComponents(BuildContext ctx)
        {
            sockets = ctx.AvatarRootObject.GetComponentsInChildren<Socket>(true);
            plugs = ctx.AvatarRootObject.GetComponentsInChildren<Plug>(true);
        }
        
        internal static void CreateComponent(BuildContext ctx)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            
            foreach (var socket in sockets)
            {
                SocketProcessor.CreateSender(animator, socket.transform, socket);
                SocketProcessor.CreateLights(socket.transform, socket.mode);
                SocketProcessor.CreateHaptics(ctx, animator, socket.transform, socket);
                if (socket.enableDepthAnimations)
                {
                    SocketProcessor.CreateVRCContacts(ctx, socket.transform, socket);
                }
                SocketProcessor.CreateAutoDistance(ctx, socket);
            }
            
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
                    SkinnedMeshRenderer skin = null;
                    try
                    {
                        skin = PlugProcessor.CreateNormalizeRenderer(renderer, bakedSpsPlug, size.worldLength);
                        
                        var spsBlendshapes = plug.animatedBlendshapes
                            .Where(b => skin.sharedMesh.GetBlendShapeIndex(b) >= 0)
                            .Distinct()
                            .Take(16)
                            .ToArray();

                        var activeFromMask = PlugProcessor.GetMask(skin, plug);
                        
                        //TODO: ここでAutoRig設定？
                        
                        var spsBaked = PlugProcessor.BakeTexture2D(skin, activeFromMask, spsBlendshapes);

                        var materials = skin.sharedMaterials;
                        for (var j = 0; j < materials.Length; j++)
                        {
                            var sourceMaterial = materials[j];
                            var (newShader, _) = ShaderPatcher.Patch(sourceMaterial.shader);

                            var material = new Material(sourceMaterial.shader);
                            material.name = sourceMaterial.name;
                            material.CopyPropertiesFromMaterial(sourceMaterial);
                            material.shader = newShader;
                            material.SetFloat("_SPS_Enabled", plug.animatedToggle ? 1f : 0f);
                            bakedSpsPlug.SetActive(plug.animatedToggle);
                            material.SetFloat("_SPS_Length", size.worldLength);
                            material.SetFloat("_SPS_BakedLength", size.worldLength);
                            material.SetFloat("_SPS_Overrun", plug.allowHoleOverrun ? 1f : 0f);
                            material.SetTexture("_SPS_Bake", spsBaked);
                            material.SetFloat("_SPS_BlendshapeCount", spsBlendshapes.Length);
                            material.SetFloat("_SPS_BlendshapeVertCount", skin.sharedMesh.vertexCount);
                            for (var i = 0; i < spsBlendshapes.Length; i++) {
                                var name = spsBlendshapes[i];
                                var id = skin.sharedMesh.GetBlendShapeIndex(name);
                                if (id >= 0) {
                                    material.SetFloat("_SPS_Blendshape" + i, skin.GetBlendShapeWeight(id));
                                }
                            }
                            ObjectRegistry.RegisterReplacedObject(sourceMaterial, material);
                            materials[j] = material;
                        }
                        skin.sharedMaterials = materials;
                    }
                    catch (Exception e)
                    {
                        NDMFConsole.LogError("ndmf.console.plug.failedToConfigureRenderer", e);
                    }
                    
                    if (skin == null) continue;
                    var scaledProps = PlugProcessor.GetScaleProps(skin.sharedMaterials);
                    if (scaledProps.Count == 0)
                    {
                        continue;
                    }
                    
                    skin.sharedMaterials = skin.sharedMaterials.Select(mat => {
                        var isTps = IsTps(mat);
                        var isSps = IsSps(mat);

                        if (!isTps && !isSps) return mat;

                        mat = Object.Instantiate(mat);
                        if (isTps) {
                            if (IsLocked(mat))
                            {
                                throw new Exception();
                            }
                            mat.SetOverrideTag("_TPS_PenetratorLengthAnimated", "1");
                            mat.SetOverrideTag("_TPS_PenetratorScaleAnimated", "1");
                        }
                        if (isSps) {
                            mat.SetOverrideTag("_SPS_LengthAnimated", "1");
                        }
                        return mat;
                    }).ToArray();
                    
                    var props = scaledProps.Select(p => (skin.gameObject, skin.GetType(), $"material.{p.Key}", p.Value));

                    PlugProcessor.CreateScaleDetector(ctx, bakedSpsPlug, plug, props);
                }
                
            }
        }

        internal static void CreateAnim(BuildContext ctx)
        {
            foreach (var socket in sockets)
            {
                int i = 0;
                foreach (var depthAction in socket.depthActions)
                {
                    SocketProcessor.CreateDepthAnims(ctx, socket, depthAction, i);
                    i++;
                }

                SocketProcessor.CreateActiveAnimations(ctx, socket, socket.activeAnimationActions);
            }
            
            foreach (var plug in plugs)
            {
                int i = 0;
                foreach (var depthAction in plug.depthActions)
                {
                    PlugProcessor.CreateDepthAnims(ctx, plug, depthAction, i);
                    i++;
                }
                PlugProcessor.CreatePostBakeActions(ctx, plug, plug.postBakeActions);
            }
        }

        /**
         * 本家がSocketのみ有効無効のメニュー自動生成っぽいのでそれに合わせる
         */
        internal static void CreateMenu(BuildContext ctx)
        {
            if (sockets.Length == 0) return;
            var spsMenusObjectRoot = new GameObject("SPS");
            spsMenusObjectRoot.transform.parent = ctx.AvatarRootTransform;
            spsMenusObjectRoot.AddComponent<ModularAvatarMenuInstaller>();
            var maRootManuItem = spsMenusObjectRoot.AddComponent<ModularAvatarMenuItem>();

            maRootManuItem.Control = new VRCExpressionsMenu.Control();
            maRootManuItem.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            maRootManuItem.MenuSource = SubmenuSource.Children;
            spsMenusObjectRoot.AddComponent<ModularAvatarMenuGroup>();
            
            foreach (var socket in sockets)
            {
                var objectName = socket.gameObject.name.Replace("/", "_");
                var socketMenuObject = new GameObject(objectName);
                socketMenuObject.transform.parent = spsMenusObjectRoot.transform;
                var maManuItem = socketMenuObject.AddComponent<ModularAvatarMenuItem>();
                maManuItem.Control = new VRCExpressionsMenu.Control();
                maManuItem.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
                maManuItem.Control.value = 1f;
                maManuItem.Control.parameter = new VRCExpressionsMenu.Control.Parameter();
                maManuItem.Control.parameter.name = $"{objectName}/Socket/Active";
            }
        }

        internal static void RemoveComponent(BuildContext ctx)
        {
            foreach (var component in ctx.AvatarRootObject.GetComponentsInChildren<SPSforNDMFTagComponent>(true))
            {
                Object.DestroyImmediate(component);
            }
        }

        internal static void Validation(BuildContext ctx)
        {
            var components = ctx.AvatarRootObject.GetComponentsInChildren<ContactBase>(true);
            if (components.Length > 256)
            {
                NDMFConsole.LogWarning("ndmf.console.contact.maximumlimit");
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
            string[] collisionTags, 
            string parameter,
            Animator animator,
            ReceiverParty party,
            bool localOnly = false, 
            float height = 0,
            Quaternion rot = default,
            ContactReceiver.ReceiverType receiverType = ContactReceiver.ReceiverType.Proximity,
            bool worldScale = true,
            bool useHipAvoidance = true)
        {
            if (party == ReceiverParty.Both)
            {
                var otherTarget = CreateParentGameObject("Others", target.transform);
                CreateVRCContactReceiver(otherTarget, radius, pos, collisionTags, $"{parameter}/Others", animator, ReceiverParty.Others, localOnly, height, rot, receiverType, worldScale, useHipAvoidance);
                var selfTarget = CreateParentGameObject("Self", target.transform);
                CreateVRCContactReceiver(selfTarget, radius, pos, collisionTags, $"{parameter}/Self", animator, ReceiverParty.Self, localOnly, height, rot, receiverType, worldScale, useHipAvoidance);
                return;
            }
            
            var receiver = target.AddComponent<VRCContactReceiver>();
            receiver.shapeType = ContactBase.ShapeType.Sphere;
            receiver.radius = radius;
            receiver.position = pos;
            receiver.allowSelf = party == ReceiverParty.Self;
            receiver.allowOthers = party == ReceiverParty.Others;
            receiver.localOnly = localOnly;
            receiver.receiverType = receiverType;
            receiver.parameter = parameter;
            
            var maMergeAnimator = target.AddComponent<ModularAvatarMergeAnimator>();
            var controller = new AnimatorController();
            controller.AddParameter(parameter, AnimatorControllerParameterType.Float);
            maMergeAnimator.animator = controller;
            maMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            maMergeAnimator.matchAvatarWriteDefaults = true;
            
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

        internal static bool IsSps(Material mat)
        {
            if (mat == null) return false;
            return mat.HasProperty("_SPS_Bake");
        }

        internal static bool IsLocked(Material mat)
        {
            if (mat == null) return false;
            if (mat.shader == null) return false;
            return mat.shader.name.ToLower().Contains("locked");
        }

        internal static Quaternion GetTpsRotation(Material mat)
        {
            if (mat.HasProperty("_TPS_PenetratorForward")) {
                var c = mat.GetVector("_TPS_PenetratorForward");
                return Quaternion.LookRotation(new Vector3(c.x, c.y, c.z));
            }
            return Quaternion.identity;
        }

        internal static (AnimationClip, AnimationClip) CreateAnimationClip(BuildContext ctx, GameObject clipRoot, List<IAction> actions, AnimatorState animatorState)
        {
            var onClip = new AnimationClip();
            var offClip = new AnimationClip();

            onClip.name = "on";
            offClip.name = "off";

            var firstClip = actions
                .OfType<AnimationClipAction>()
                .Select(action => action.clip)
                .FirstOrDefault();

            if (firstClip)
            {
                var copy = Object.Instantiate(firstClip);
                copy.name = onClip.name;
                onClip = copy;
            }
            
            foreach (var action in actions)
            {
                switch (action)
                {
                    case AnimationClipAction clipAction:
                    {
                        if (clipAction.clip == null || clipAction.clip == firstClip) break;
                        var copy = Object.Instantiate(clipAction.clip);
                        onClip = copy;
                        
                        break;
                    }
                    case ObjectToggleAction objectToggleAction:
                    {
                        if (objectToggleAction.obj == null) break;
                        var onState = true;
                        if (objectToggleAction.mode == ObjectToggleAction.Mode.TurnOff)
                        {
                            onState = false;
                        }
                        else if (objectToggleAction.mode == ObjectToggleAction.Mode.Toggle)
                        {
                            onState = !objectToggleAction.obj.activeSelf;
                        }
                        
                        var curveBinding = new EditorCurveBinding();
                        
                        curveBinding.path = AnimationUtility.CalculateTransformPath(objectToggleAction.obj.transform, clipRoot.transform);
                        curveBinding.type = typeof(GameObject);
                        curveBinding.propertyName = "m_IsActive";

                        var onCurve = new AnimationCurve();
                        onCurve.AddKey(0f, onState ? 1 : 0);

                        AnimationUtility.SetEditorCurve(onClip, curveBinding, onCurve);
                    
                        var offCurve = new AnimationCurve();
                        offCurve.AddKey(0f, !onState ? 1 : 0);

                        AnimationUtility.SetEditorCurve(offClip, curveBinding, offCurve);
                        
                        break;
                    }
                    case BlendShapeAction blendShape:
                    {
                        if (string.IsNullOrEmpty(blendShape.blendShape)) break;
                        foreach (var skin in ctx.AvatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                        {
                            if (!blendShape.allRenderers && blendShape.renderer != skin) continue;
                            if (!(skin.sharedMesh.GetBlendShapeIndex(blendShape.blendShape) >= 0)) continue;
                            
                            var curveBinding = new EditorCurveBinding();

                            curveBinding.path = ctx.AvatarRootObject.name;
                            curveBinding.type = typeof(SkinnedMeshRenderer);
                            curveBinding.propertyName = $"blendShape.{blendShape.blendShape}";

                            var onCurve = new AnimationCurve();
                            onCurve.AddKey(0f, blendShape.blendShapeValue);

                            AnimationUtility.SetEditorCurve(onClip, curveBinding, onCurve);
                        }
                        break;
                    }
                    case FxFloatAction fxFloatAction:
                    {
                        if (string.IsNullOrWhiteSpace(fxFloatAction.name)) break;
                        var parameterDriver = animatorState.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                        parameterDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter()
                        {
                            name = fxFloatAction.name,
                            value = fxFloatAction.value
                        });
                        break;
                    }
                }
            }
            return (onClip, offClip);
        }
    }
}
