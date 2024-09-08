using System;
using System.Collections.Generic;
using System.Linq;
using com.meronmks.ndmfsps;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor.Graphs;
using UnityEngine;
using VRC.Dynamics;
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
                    Quaternion.identity,
                    true,
                    Processor.selfContacts,
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelf.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Self);
                
                Processor.CreateVRCContactReceiver(
                    touchSelfClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    Quaternion.Euler(90f,0f,0f),
                    true,
                    Processor.selfContacts,
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelfClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Self,
                    height: handTouchZone.length);
                
                Processor.CreateVRCContactReceiver(
                    touchOthers,
                    handTouchZone.length,
                    Vector3.forward * -handTouchZone.length,
                    Quaternion.identity,
                    true,
                    Processor.bodyContacts,
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others);
                
                Processor.CreateVRCContactReceiver(
                    touchOthersClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    Quaternion.Euler(90f,0f,0f),
                    true,
                    Processor.bodyContacts,
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthersClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    height: handTouchZone.length);
                
                Processor.CreateVRCContactReceiver(
                    penOthers,
                    handTouchZone.length,
                    Vector3.forward * -handTouchZone.length,
                    Quaternion.identity,
                    true,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others);
                
                Processor.CreateVRCContactReceiver(
                    penOthersClose,
                    handTouchZone.radius,
                    Vector3.forward * -(handTouchZone.length/2),
                    Quaternion.Euler(90f,0f,0f),
                    true,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersClose.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others,
                    height: handTouchZone.length);
                
                Processor.CreateVRCContactReceiver(
                    frotOthers,
                    0.1f,
                    Vector3.forward * 0.05f,
                    Quaternion.identity,
                    true,
                    new []
                    {
                        "TPS_Orf_Root"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{frotOthers.name.Replace("/", "_")}",
                    animator,
                    Processor.ReceiverParty.Others);
            }
            
            Processor.CreateVRCContactReceiver(
                penSelfNewRoot,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                new []
                {
                    "TPS_Pen_Root"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewRoot.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self);
            
            Processor.CreateVRCContactReceiver(
                penSelfNewTip,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewTip.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self);
            
            Processor.CreateVRCContactReceiver(
                penOthersNewRoot,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                new []
                {
                    "TPS_Pen_Root"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewRoot.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others);
            
            Processor.CreateVRCContactReceiver(
                penOthersNewTip,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewTip.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others);
        }
        
        /// <summary>
        /// Plugの位置によってアニメーションさせる奴
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAnimations(BuildContext ctx, Transform root, ICollection<DepthAction> actions)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            
            var maxDist = Math.Max(0, actions.Max(a => Math.Max(a.startDistance, a.endDistance)));
            var minDist = Math.Min(0, actions.Min(a => Math.Min(a.startDistance, a.endDistance)));
            var offset = Math.Max(0, -minDist);
            
            var animationsRoot = Processor.CreateParentGameObject("Animations", root);
            var outerGameObject = Processor.CreateParentGameObject("Outer", animationsRoot.transform);
            var frontOthersGameObject = Processor.CreateParentGameObject("FrontOthers", outerGameObject.transform);
            var frontSelfGameObject = Processor.CreateParentGameObject("FrontSelf", outerGameObject.transform);
            var backOthersGameObject = Processor.CreateParentGameObject("BackOthers", outerGameObject.transform);
            var backSelfGameObject = Processor.CreateParentGameObject("BackSelf", outerGameObject.transform);
            
            //TODO: 以下の生成がまだ一部固定値
            Processor.CreateVRCContactReceiver(
                frontOthersGameObject,
                0.03f,
                Vector3.zero,
                Quaternion.identity,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "Front",
                animator,
                Processor.ReceiverParty.Others);
            Processor.CreateVRCContactReceiver(
                frontSelfGameObject,
                0.03f,
                Vector3.zero,
                Quaternion.identity,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "Front",
                animator,
                Processor.ReceiverParty.Others);
            Processor.CreateVRCContactReceiver(
                backOthersGameObject,
                0.03f,
                Vector3.zero,
                Quaternion.identity,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "Back",
                animator,
                Processor.ReceiverParty.Others);
            Processor.CreateVRCContactReceiver(
                backSelfGameObject,
                0.03f,
                Vector3.zero,
                Quaternion.identity,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "Back",
                animator,
                Processor.ReceiverParty.Others);
            if (minDist < 0)
            {
                var innerGameObject = Processor.CreateParentGameObject("Inner", animationsRoot.transform);
                var frontOthersInnerGameObject = Processor.CreateParentGameObject("FrontOthers", innerGameObject.transform);
                var frontSelfInnerGameObject = Processor.CreateParentGameObject("FrontSelf", innerGameObject.transform);
                var backOthersInnerGameObject = Processor.CreateParentGameObject("BackOthers", innerGameObject.transform);
                var backSelfInnerGameObject = Processor.CreateParentGameObject("BackSelf", innerGameObject.transform);
                Processor.CreateVRCContactReceiver(
                    frontOthersInnerGameObject,
                    0.03f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "Front",
                    animator,
                    Processor.ReceiverParty.Others);
                Processor.CreateVRCContactReceiver(
                    frontSelfInnerGameObject,
                    0.03f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "Front",
                    animator,
                    Processor.ReceiverParty.Others);
                Processor.CreateVRCContactReceiver(
                    backOthersInnerGameObject,
                    0.03f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "Back",
                    animator,
                    Processor.ReceiverParty.Others);
                Processor.CreateVRCContactReceiver(
                    backSelfInnerGameObject,
                    0.03f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "Back",
                    animator,
                    Processor.ReceiverParty.Others);
            }
        }
        
        /// <summary>
        /// Plugが接近したら自動でOnになる機能に使われてるっぽい
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAutoDistance(BuildContext ctx, Transform root)
        {
            var animator = ctx.AvatarRootObject.GetComponent<Animator>();
            var autoDistanceRoot = Processor.CreateParentGameObject("AutoDistance", root);
            var receiverGameObject = Processor.CreateParentGameObject("Receiver", autoDistanceRoot.transform);
            
            Processor.CreateVRCContactReceiver(
                receiverGameObject,
                0.3f,
                Vector3.zero,
                Quaternion.identity,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "",
                animator,
                Processor.ReceiverParty.Others);
        }
    }
}