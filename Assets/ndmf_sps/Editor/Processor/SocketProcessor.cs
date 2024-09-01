using System;
using System.Collections.Generic;
using System.Linq;
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
        private static Color spsTypeColor = new Color(0, 0, 0, 255);
        
        private const string SENDER_PARAMPREFIX = "OGB/Orf/";
        
        /// <summary>
        /// TPSとSPSで使う何かへのSender
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateSender(Transform root)
        {
            var senderObject = Processor.CreateParentGameObject("Senders", root);
            var rootObject = Processor.CreateParentGameObject("Root", senderObject.transform);
            var frontObject = Processor.CreateParentGameObject("Front", senderObject.transform);

            Processor.CreateVRCContactSender(rootObject, 
                ContactBase.ShapeType.Sphere, 
                0.001f, 
                Vector3.zero, 
                Quaternion.identity, 
                new []
                {
                    "TPS_Orf_Root",
                    "TPS_Orf_Root_SelfNotOnHips",
                    "SPSLL_Socket_Root",
                    "SPSLL_Socket_Root_SelfNotOnHips",
                    "SPSLL_Socket_Hole",
                    "SPSLL_Socket_Hole_SelfNotOnHips"
                });
            
            Processor.CreateVRCContactSender(frontObject, 
                ContactBase.ShapeType.Sphere, 
                0.001f, 
                Vector3.forward * 0.01f, 
                Quaternion.identity, 
                new []
                {
                    "TPS_Orf_Norm",
                    "TPS_Orf_Norm_SelfNotOnHips",
                    "SPSLL_Socket_Front",
                    "SPSLL_Socket_Front_SelfNotOnHips"
                });
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
            frontObject.gameObject.transform.SetLocalPositionAndRotation(Vector3.forward * 0.01f, Quaternion.identity);

            var lightRootComponent = rootObject.AddComponent<Light>();
            var lightFrontComponent = frontObject.AddComponent<Light>();

            switch (mode)
            {
                case Socket.SocketMode.Hole:
                    lightRootComponent.range = 0.4102f;
                    break;
                case Socket.SocketMode.Ring:
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
        /// なんだろ・・・？謎
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateHaptics(Transform root, Socket.Haptics haptics)
        {
            var hapticsRoot = Processor.CreateParentGameObject("Haptics", root);
            var penSelfNewRoot = Processor.CreateParentGameObject("PenSelfNewRoot", hapticsRoot.transform);
            var penSelfNewTip = Processor.CreateParentGameObject("PenSelfNewTip", hapticsRoot.transform);
            var penOthersNewRoot = Processor.CreateParentGameObject("PenOthersNewRoot", hapticsRoot.transform);
            var penOthersNewTip = Processor.CreateParentGameObject("PenOthersNewTip", hapticsRoot.transform);

            if (haptics == Socket.Haptics.On)
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
                    ContactBase.ShapeType.Sphere,
                    0.052f,
                    1f,
                    Vector3.back * 0.052f,
                    Quaternion.identity,
                    true,
                    false,
                    true,
                    new []
                    {
                        "Hand",
                        "Finger",
                        "Foot"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelf.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    touchSelfClose,
                    ContactBase.ShapeType.Capsule,
                    0.0208f,
                    0.052f,
                    Vector3.back * 0.026f,
                    Quaternion.Euler(90f,0f,0f),
                    true,
                    false,
                    true,
                    new []
                    {
                        "Hand",
                        "Finger",
                        "Foot"
                    },
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelfClose.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    touchOthers,
                    ContactBase.ShapeType.Sphere,
                    0.052f,
                    1f,
                    Vector3.back * 0.052f,
                    Quaternion.identity,
                    false,
                    true,
                    true,
                    new []
                    {
                        "Head",
                        "Hand",
                        "Finger",
                        "Foot"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthers.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    touchOthersClose,
                    ContactBase.ShapeType.Capsule,
                    0.0208f,
                    0.052f,
                    Vector3.back * 0.026f,
                    Quaternion.Euler(90f,0f,0f),
                    false,
                    true,
                    true,
                    new []
                    {
                        "Head",
                        "Hand",
                        "Finger",
                        "Foot"
                    },
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthersClose.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    penOthers,
                    ContactBase.ShapeType.Sphere,
                    0.052f,
                    1f,
                    Vector3.back * 0.052f,
                    Quaternion.identity,
                    false,
                    true,
                    true,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthers.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    penOthersClose,
                    ContactBase.ShapeType.Capsule,
                    0.0208f,
                    0.052f,
                    Vector3.back * 0.026f,
                    Quaternion.Euler(90f,0f,0f),
                    false,
                    true,
                    true,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Constant,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersClose.name.Replace("/", "_")}");
                
                Processor.CreateVRCContactReceiver(
                    frotOthers,
                    ContactBase.ShapeType.Sphere,
                    0.1f,
                    1f,
                    Vector3.forward * 0.05f,
                    Quaternion.identity,
                    false,
                    true,
                    true,
                    new []
                    {
                        "TPS_Orf_Root"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{frotOthers.name.Replace("/", "_")}");
            }
            
            Processor.CreateVRCContactReceiver(
                penSelfNewRoot,
                ContactBase.ShapeType.Sphere,
                1f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                false,
                true,
                new []
                {
                    "TPS_Pen_Root"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewRoot.name.Replace("/", "_")}");
            
            Processor.CreateVRCContactReceiver(
                penSelfNewTip,
                ContactBase.ShapeType.Sphere,
                1f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                false,
                true,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfNewTip.name.Replace("/", "_")}");
            
            Processor.CreateVRCContactReceiver(
                penOthersNewRoot,
                ContactBase.ShapeType.Sphere,
                1f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                false,
                true,
                true,
                new []
                {
                    "TPS_Pen_Root"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewRoot.name.Replace("/", "_")}");
            
            Processor.CreateVRCContactReceiver(
                penOthersNewTip,
                ContactBase.ShapeType.Sphere,
                1f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                false,
                true,
                true,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersNewTip.name.Replace("/", "_")}");
        }
        
        /// <summary>
        /// Plugの位置によってアニメーションさせる奴
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAnimations(Transform root, ICollection<DepthAction> actions)
        {
            var maxDist = Math.Max(0, actions.Max(a => Math.Max(a.startDistance, a.endDistance)));
            var minDist = Math.Min(0, actions.Min(a => Math.Min(a.startDistance, a.endDistance)));
            var offset = Math.Max(0, -minDist);
            
            var animationsRoot = Processor.CreateParentGameObject("Animations", root);
            var outerGameObject = Processor.CreateParentGameObject("Outer", animationsRoot.transform);
            var frontOthersGameObject = Processor.CreateParentGameObject("FrontOthers", outerGameObject.transform);
            var frontSelfGameObject = Processor.CreateParentGameObject("FrontSelf", outerGameObject.transform);
            var backOthersGameObject = Processor.CreateParentGameObject("BackOthers", outerGameObject.transform);
            var backSelfGameObject = Processor.CreateParentGameObject("BackSelf", outerGameObject.transform);
            Processor.CreateVRCContactReceiver(
                frontOthersGameObject,
                ContactBase.ShapeType.Sphere,
                0.03f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                false,
                true,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "");
            Processor.CreateVRCContactReceiver(
                frontSelfGameObject,
                ContactBase.ShapeType.Sphere,
                0.03f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                false,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "");
            Processor.CreateVRCContactReceiver(
                backOthersGameObject,
                ContactBase.ShapeType.Sphere,
                0.03f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                false,
                true,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "");
            Processor.CreateVRCContactReceiver(
                backSelfGameObject,
                ContactBase.ShapeType.Sphere,
                0.03f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                true,
                false,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "");
            if (minDist < 0)
            {
                var innerGameObject = Processor.CreateParentGameObject("Inner", animationsRoot.transform);
                var frontOthersInnerGameObject = Processor.CreateParentGameObject("FrontOthers", innerGameObject.transform);
                var frontSelfInnerGameObject = Processor.CreateParentGameObject("FrontSelf", innerGameObject.transform);
                var backOthersInnerGameObject = Processor.CreateParentGameObject("BackOthers", innerGameObject.transform);
                var backSelfInnerGameObject = Processor.CreateParentGameObject("BackSelf", innerGameObject.transform);
                Processor.CreateVRCContactReceiver(
                    frontOthersInnerGameObject,
                    ContactBase.ShapeType.Sphere,
                    0.03f,
                    1f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    true,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "");
                Processor.CreateVRCContactReceiver(
                    frontSelfInnerGameObject,
                    ContactBase.ShapeType.Sphere,
                    0.03f,
                    1f,
                    Vector3.zero,
                    Quaternion.identity,
                    true,
                    false,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "");
                Processor.CreateVRCContactReceiver(
                    backOthersInnerGameObject,
                    ContactBase.ShapeType.Sphere,
                    0.03f,
                    1f,
                    Vector3.zero,
                    Quaternion.identity,
                    false,
                    true,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "");
                Processor.CreateVRCContactReceiver(
                    backSelfInnerGameObject,
                    ContactBase.ShapeType.Sphere,
                    0.03f,
                    1f,
                    Vector3.zero,
                    Quaternion.identity,
                    true,
                    false,
                    false,
                    new []
                    {
                        "TPS_Pen_Penetrating"
                    },
                    ContactReceiver.ReceiverType.Proximity,
                    //TODO: 決まったパラメータはどこから来てるのか調べる。
                    "");
            }
        }
        
        /// <summary>
        /// Plugが接近したら自動でOnになる機能に使われてるっぽい
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAutoDistance(Transform root)
        {
            var autoDistanceRoot = Processor.CreateParentGameObject("AutoDistance", root);
            var receiverGameObject = Processor.CreateParentGameObject("Receiver", autoDistanceRoot.transform);
            
            Processor.CreateVRCContactReceiver(
                receiverGameObject,
                ContactBase.ShapeType.Sphere,
                0.3f,
                1f,
                Vector3.zero,
                Quaternion.identity,
                false,
                true,
                false,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                ContactReceiver.ReceiverType.Proximity,
                //TODO: 決まったパラメータはどこから来てるのか調べる。
                "");
        }
    }
}