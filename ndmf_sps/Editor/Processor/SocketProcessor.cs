using System;
using System.Collections.Generic;
using System.Linq;
using com.meronmks.ndmfsps;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor.Graphs;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Dynamics.Contact.Components;

namespace com.meronmks.ndmfsps
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class SocketProcessor
    {
        // SPSシェーダが対象のLightだと判定する色
        private static Color spsTypeColor = Color.black;
        
        private const string SENDER_PARAMPREFIX = "OGB/Orf/";
        
        /// <summary>
        /// TPSとSPSで使う何かへのSender
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateSender(Animator animator, Transform root, Socket socket)
        {
            var senderObject = Processor.CreateParentGameObject("Senders", root);
            var rootObject = Processor.CreateParentGameObject("Root", senderObject.transform);
            var frontObject = Processor.CreateParentGameObject("Front", senderObject.transform);

            var rootTags = new List<string>();
            rootTags.Add("TPS_Orf_Root");
            rootTags.Add("SPSLL_Socket_Root");

            if (socket.mode != Socket.SocketMode.None && !socket.sendersOnly)
            {
                switch (socket.mode)
                {
                    case Socket.SocketMode.Ring:
                        rootTags.Add("SPSLL_Socket_Ring");
                        break;
                    case Socket.SocketMode.RingOneWay:
                        rootTags.Add("SPSLL_Socket_Ring");
                        rootTags.Add("SPSLL_Socket_Hole");
                        break;
                    default:
                        rootTags.Add("SPSLL_Socket_Hole");
                        break;
                }
            }
            
            Processor.CreateVRCContactSender(rootObject,
                0.001f, 
                Vector3.zero, 
                Quaternion.identity, 
                rootTags.ToArray(),
                animator);
            
            Processor.CreateVRCContactSender(frontObject,
                0.001f, 
                Vector3.forward * 0.01f, 
                Quaternion.identity, 
                new []
                {
                    "TPS_Orf_Norm",
                    "SPSLL_Socket_Front"
                },
                animator);
        }
        
        /// <summary>
        /// Plugの目標となるLightの生成
        /// </summary>
        /// <param name="root"></param>
        /// <param name="mode"></param>
        internal static void CreateLights(Transform root, Socket.SocketMode mode)
        {
            var lightRoot = Processor.CreateParentGameObject("Lights", root);
            var rootObject = Processor.CreateParentGameObject("Root", lightRoot.transform);
            var frontObject = Processor.CreateParentGameObject("Front", lightRoot.transform);
            frontObject.gameObject.transform.SetLocalPositionAndRotation(Vector3.forward * 0.01f / lightRoot.transform.lossyScale.x, Quaternion.identity);

            var lightRootComponent = rootObject.AddComponent<Light>();
            var lightFrontComponent = frontObject.AddComponent<Light>();

            lightRootComponent.type = LightType.Point;
            lightFrontComponent.type = LightType.Point;
            lightRootComponent.shadows = LightShadows.None;
            lightFrontComponent.shadows = LightShadows.None;

            switch (mode)
            {
                case Socket.SocketMode.Hole:
                    lightRootComponent.range = 0.4102f;
                    break;
                case Socket.SocketMode.Ring:
                case Socket.SocketMode.RingOneWay:
                    lightRootComponent.range = 0.4202f;
                    break;
            }
            lightFrontComponent.range = 0.4502f;

            lightRootComponent.color = spsTypeColor;
            lightFrontComponent.color = spsTypeColor;
            lightRootComponent.renderMode = LightRenderMode.ForceVertex;
            lightFrontComponent.renderMode = LightRenderMode.ForceVertex;
        }

        /// <summary>
        /// 対象の何かが触れたときに発火するHapticsレシーバーたち
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateHaptics(BuildContext ctx, Animator animator, Transform root, Socket socket)
        {
            var hapticsRoot = Processor.CreateParentGameObject("Haptics", root);
            var penSelfNewRoot = Processor.CreateParentGameObject("PenSelfNewRoot", hapticsRoot.transform);
            var penSelfNewTip = Processor.CreateParentGameObject("PenSelfNewTip", hapticsRoot.transform);
            var penOthersNewRoot = Processor.CreateParentGameObject("PenOthersNewRoot", hapticsRoot.transform);
            var penOthersNewTip = Processor.CreateParentGameObject("PenOthersNewTip", hapticsRoot.transform);

            var handTouchZone = HandTouchZone.GetHandTouchZoneSize(socket, ctx.AvatarDescriptor);
            if (handTouchZone != null)
            {
                var touchSelf = Processor.CreateParentGameObject("TouchSelf", hapticsRoot.transform);
                var touchSelfClose = Processor.CreateParentGameObject("TouchSelfClose", hapticsRoot.transform);
                var touchOthers = Processor.CreateParentGameObject("TouchOthers", hapticsRoot.transform);
                var touchOthersClose = Processor.CreateParentGameObject("TouchOthersClose", hapticsRoot.transform);
                var penOthers = Processor.CreateParentGameObject("PenOthers", hapticsRoot.transform);
                var penOthersClose = Processor.CreateParentGameObject("PenOthersClose", hapticsRoot.transform);
                var frotOthers = Processor.CreateParentGameObject("FrotOthers", hapticsRoot.transform); //Typoっぽいが元がこうなので一旦これで
                
                Processor.CreateVRCContactReceiver(
                    touchSelf,
                    handTouchZone.length,
                    Vector3.forward * -handTouchZone.length,
                    Processor.selfContacts,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelf.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Self,
                    localOnly: true,
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    touchSelfClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    Processor.selfContacts,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelfClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Self,
                    receiverType: ContactReceiver.ReceiverType.Constant,
                    height: handTouchZone.length,
                    localOnly: true,
                    rot: Quaternion.Euler(90,0,0),
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    touchOthers,
                    handTouchZone.length,
                    Vector3.forward * -handTouchZone.length,
                    Processor.bodyContacts,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    localOnly: true,
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    touchOthersClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    Processor.bodyContacts,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthersClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    receiverType: ContactReceiver.ReceiverType.Constant,
                    localOnly: true,
                    height: handTouchZone.length,
                    rot: Quaternion.Euler(90,0,0),
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    penOthers,
                    handTouchZone.length,
                    Vector3.forward * -handTouchZone.length,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    localOnly: true,
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    penOthersClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    receiverType: ContactReceiver.ReceiverType.Constant,
                    localOnly: true,
                    height: handTouchZone.length,
                    rot: Quaternion.Euler(90,0,0),
                    useHipAvoidance: socket.useHipAvoidance);
                
                Processor.CreateVRCContactReceiver(
                    frotOthers,
                    0.1f,
                    Vector3.forward * 0.05f,
                    new []
                    {
                        "TPS_Orf_Root"
                    },
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{frotOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    localOnly: true,
                    useHipAvoidance: socket.useHipAvoidance);
            }
            
            Processor.CreateVRCContactReceiver(
                penSelfNewRoot,
                1f,
                Vector3.zero,
                new []
                {
                    "TPS_Pen_Root"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewRoot.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self,
                localOnly: true,
                useHipAvoidance: socket.useHipAvoidance);
            
            Processor.CreateVRCContactReceiver(
                penSelfNewTip,
                1f,
                Vector3.zero,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewTip.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self,
                localOnly: true,
                useHipAvoidance: socket.useHipAvoidance);
            
            Processor.CreateVRCContactReceiver(
                penOthersNewRoot,
                1f,
                Vector3.zero,
                new []
                {
                    "TPS_Pen_Root"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewRoot.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                localOnly: true,
                useHipAvoidance: socket.useHipAvoidance);
            
            Processor.CreateVRCContactReceiver(
                penOthersNewTip,
                1f,
                Vector3.zero,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewTip.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                localOnly: true,
                useHipAvoidance: socket.useHipAvoidance);
        }
        
        /// <summary>
        /// Plugの位置によって動くContactとAAP距離計算
        /// </summary>
        internal static void CreateVRCContacts(BuildContext ctx, Transform root, Socket socket)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            if (socket.depthActions.Count == 0) return;
            var objectName = root.gameObject.name.Replace("/", "_");
            var scale = socket.unitsInMeters ? 1f : root.lossyScale.z;

            var maxDist = Math.Max(0, socket.depthActions.Max(a => Math.Max(a.startDistance, a.endDistance))) * scale;
            var minDist = Math.Min(0, socket.depthActions.Min(a => Math.Min(a.startDistance, a.endDistance))) * scale;
            var offset = Math.Max(0, -minDist);

            var controller = new AnimatorController();
            var math = new AapMath(controller, objectName);

            // enableSelf ごとに距離計算用のContactとAAPを作成
            var distanceParams = new Dictionary<bool, string>();

            var enableSelfValues = socket.depthActions.Select(a => a.enableSelf).Distinct();
            foreach (var enableSelf in enableSelfValues)
            {
                var animParmPrefix = $"{objectName}/Anim{(enableSelf ? "" : "Others")}";
                var outerRadius = Math.Max(0.01f, maxDist);
                var party = enableSelf ? Processor.ReceiverParty.BothCombined : Processor.ReceiverParty.Others;

                // Outer Front contact
                var animationsRoot = Processor.CreateParentGameObject("Animations", root);
                var outerGameObject = Processor.CreateParentGameObject("Outer", animationsRoot.transform);
                var frontGameObject = Processor.CreateParentGameObject("Front", outerGameObject.transform);

                var outerFrontParam = $"{animParmPrefix}/Outer/Front";
                Processor.CreateVRCContactReceiver(
                    frontGameObject,
                    outerRadius,
                    Vector3.zero,
                    new[] { "TPS_Pen_Penetrating" },
                    outerFrontParam,
                    animator,
                    party,
                    useHipAvoidance: socket.useHipAvoidance);

                // Inner Front contact (負距離がある場合のみ)
                string innerFrontParam = null;
                if (minDist < 0)
                {
                    var innerGameObject = Processor.CreateParentGameObject("Inner", animationsRoot.transform);
                    var frontInnerGameObject = Processor.CreateParentGameObject("Front", innerGameObject.transform);

                    innerFrontParam = $"{animParmPrefix}/Inner/Front";
                    Processor.CreateVRCContactReceiver(
                        frontInnerGameObject,
                        -minDist,
                        Vector3.forward * minDist,
                        new[] { "TPS_Pen_Penetrating" },
                        innerFrontParam,
                        animator,
                        party,
                        useHipAvoidance: socket.useHipAvoidance);
                }

                // ---- AAP距離計算 ----
                var distanceParam = $"{animParmPrefix}/Distance";
                math.MakeAap(distanceParam, offset + outerRadius);

                var targets = new List<(Motion motion, AapMath.ConditionFactory condition)>();

                if (minDist < 0 && innerFrontParam != null)
                {
                    // Inner距離: Plugが完全にSocket内部 (outer.front >= 1)
                    var innerDistParam = math.Map(
                        $"{animParmPrefix}/Inner/Distance",
                        innerFrontParam,
                        1f, 0f,
                        offset + minDist, offset + 0f);

                    targets.Add((
                        math.MakeCopier(innerDistParam, distanceParam),
                        math.GreaterThan(outerFrontParam, 1f, orEqual: true)
                    ));
                }

                if (maxDist > 0)
                {
                    // Outer距離: Plugが外側にいる (outer.front > 0)
                    var outerDistParam = math.Map(
                        $"{animParmPrefix}/Outer/Distance",
                        outerFrontParam,
                        1f, 0f,
                        offset + 0f, offset + outerRadius);

                    targets.Add((
                        math.MakeCopier(outerDistParam, distanceParam),
                        math.GreaterThan(outerFrontParam, 0f)
                    ));
                }

                // フォールバック: 最大距離
                targets.Add((
                    math.MakeConstSetter(distanceParam, offset + outerRadius),
                    null
                ));

                math.SetValueWithConditions(targets.ToArray());

                distanceParams[enableSelf] = distanceParam;
            }

            // 各DepthActionのMappedパラメータを計算
            for (var i = 0; i < socket.depthActions.Count; i++)
            {
                var depthAction = socket.depthActions[i];
                var mappedParam = $"{objectName}/Anim{i}/Mapped";
                var distanceParam = distanceParams[depthAction.enableSelf];

                if (depthAction.smoothingSeconds > 0)
                {
                    var rawMappedParam = $"{objectName}/Anim{i}/RawMapped";
                    math.Map(
                        rawMappedParam,
                        distanceParam,
                        offset + depthAction.startDistance * scale,
                        offset + depthAction.endDistance * scale,
                        0f, 1f);
                    math.Smooth(mappedParam, rawMappedParam, depthAction.smoothingSeconds);
                }
                else
                {
                    math.Map(
                        mappedParam,
                        distanceParam,
                        offset + depthAction.startDistance * scale,
                        offset + depthAction.endDistance * scale,
                        0f, 1f);
                }
            }

            // AnimatorControllerにレイヤーを設定しMergeAnimatorでアタッチ
            math.FinalizeController($"SPS Depth Calc for {objectName}");

            var maMergeAnimator = root.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            maMergeAnimator.animator = controller;
            maMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            maMergeAnimator.matchAvatarWriteDefaults = true;
        }
        
        /// <summary>
        /// Plugが接近したら自動でOnになる機能に使われてるっぽい
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAutoDistance(BuildContext ctx, Socket socket)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            var autoDistanceRoot = Processor.CreateParentGameObject("AutoDistance", socket.transform);
            var receiverGameObject = Processor.CreateParentGameObject("Receiver", autoDistanceRoot.transform);
            
            Processor.CreateVRCContactReceiver(
                receiverGameObject,
                0.3f,
                Vector3.zero,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                $"{socket.gameObject.name}/AutoDistance",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: socket.useHipAvoidance);
        }

        internal static void CreateDepthAnims(BuildContext ctx, Socket socket, DepthAction depthAction, int count)
        {
            if (!socket.enableDepthAnimations || socket.depthActions.Count == 0) return;
            var objectName = socket.gameObject.name.Replace("/", "_");
            var maMergeAnimator = socket.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            var controller = new AnimatorController();
            var emptyClip = new AnimationClip();
            var parmName = $"{objectName}/Anim{count}/Mapped";
            
            emptyClip.name = "Empty";
            controller.AddParameter(parmName, AnimatorControllerParameterType.Float);
            controller.AddLayer($"Depth Animation {count} for {objectName}");
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;

            var offState = stateMachine.AddState("Off");
            var onState = stateMachine.AddState("On");

            offState.motion = emptyClip;
            
            var animClipTuple = Processor.CreateAnimationClip(ctx, socket.gameObject, depthAction.actions, onState);
            var onClip = animClipTuple.Item1;
            var offClip = animClipTuple.Item2;

            if (!IsStaticClip(onClip))
            {
                // Non-static clip: MotionTimeで駆動（タイムライン上の位置をMappedで制御）
                var settings = AnimationUtility.GetAnimationClipSettings(onClip);
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(onClip, settings);

                onState.motion = onClip;
                onState.timeParameterActive = true;
                onState.timeParameter = parmName;
            }
            else
            {
                // Static clip: BlendTreeで0→off, 1→onをブレンド
                var blendTree = new BlendTree();
                blendTree.name = $"{objectName}Tree {count}";
                blendTree.blendType = BlendTreeType.Simple1D;
                blendTree.blendParameter = parmName;
                blendTree.useAutomaticThresholds = false;
                blendTree.AddChild(offClip, 0f);
                blendTree.AddChild(onClip, 1f);

                onState.motion = blendTree;
            }

            var onTransition = offState.AddTransition(onState);
            var offTransition = onState.AddTransition(offState);
            
            onTransition.AddCondition(AnimatorConditionMode.Greater, 0.01f, parmName);
            offTransition.AddCondition(AnimatorConditionMode.Less, 0.01f, parmName);
            onTransition.hasFixedDuration = true;
            offTransition.hasFixedDuration = true;
            onTransition.duration = 0f;
            offTransition.duration = 0f;
            onTransition.offset = 0f;
            offTransition.offset = 0f;

            maMergeAnimator.animator = controller;
            maMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            maMergeAnimator.matchAvatarWriteDefaults = true;
        }

        /// <summary>
        /// AnimationClipが静的（全カーブの全キーフレームがtime=0かつ同一値）かどうかを判定する。
        /// VRCFuryのMotionExtensions.IsStatic相当。
        /// </summary>
        private static bool IsStaticClip(AnimationClip clip)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                if (curve == null) continue;
                var keys = curve.keys;
                if (keys.Length == 0) continue;
                if (keys.All(key => key.time != 0)) return false;
                if (keys.Select(k => k.value).Distinct().Count() > 1) return false;
            }
            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                var keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                if (keys == null) continue;
                if (keys.Length == 0) continue;
                if (keys.All(key => key.time != 0)) return false;
                if (keys.Select(k => k.value).Distinct().Count() > 1) return false;
            }
            return true;
        }

        internal static void CreateActiveAnimations(BuildContext ctx, Socket socket, List<IAction> actions)
        {
            if (!socket.enableActiveAnimation)
            {
                actions = new List<IAction>();
            }
            var objectName = socket.gameObject.name.Replace("/", "_");
            var maMergeAnimator = socket.gameObject.AddComponent<ModularAvatarMergeAnimator>();
            var controller = new AnimatorController();
            var parmName = $"{objectName}/Socket/Active"; //TODO: パラメータ名は一旦仮置き
            
            controller.AddParameter(parmName, AnimatorControllerParameterType.Bool);
            controller.AddLayer($"SPS - Socket - Active Animation for {objectName}");
            
            var layer = controller.layers[0];
            var stateMachine = layer.stateMachine;

            var offState = stateMachine.AddState("Off");
            var onState = stateMachine.AddState("On");
            
            foreach (Transform child in socket.transform)
            {
                child.gameObject.SetActive(false);
                if (child.gameObject.name.Equals("Senders") ||
                    child.gameObject.name.Equals("Lights") ||
                    child.gameObject.name.Equals("Haptics") ||
                    child.gameObject.name.Equals("Animations"))
                {
                    actions.Add(new ObjectToggleAction
                    {
                        mode = ObjectToggleAction.Mode.TurnOn,
                        obj = child.gameObject
                    });
                }
            }
            
            var animClipTuple = Processor.CreateAnimationClip(ctx, socket.gameObject, actions, onState);

            onState.motion = animClipTuple.Item1;
            offState.motion = animClipTuple.Item2;
            
            var onTransition = offState.AddTransition(onState);
            var offTransition = onState.AddTransition(offState);
            
            onTransition.AddCondition(AnimatorConditionMode.If, 0f, parmName);
            offTransition.AddCondition(AnimatorConditionMode.IfNot, 0f, parmName);
            onTransition.hasFixedDuration = true;
            offTransition.hasFixedDuration = true;
            onTransition.duration = 0f;
            offTransition.duration = 0f;
            onTransition.offset = 0f;
            offTransition.offset = 0f;

            maMergeAnimator.animator = controller;
            maMergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            maMergeAnimator.matchAvatarWriteDefaults = true;
        }
    }
}