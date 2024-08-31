using UnityEditor.Graphs;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;

namespace com.meronmks.spsforndmf
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class SocketProcessor
    {
        // SPSシェーダが対象のLightだと判定する色
        private static Color spsTypeColor = new Color(0, 0, 0, 255);
        
        /// <summary>
        /// TPSとSPSで使う何かへのSender
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateSender(Transform root)
        {
            var senderObject = Processor.CreateParentGameObject("Senders", root);
            var rootObject = Processor.CreateParentGameObject("Root", senderObject.transform);
            var frontObject = Processor.CreateParentGameObject("Front", senderObject.transform);

            var senderRootContact = rootObject.AddComponent<VRCContactSender>();
            var senderFrontContact = frontObject.AddComponent<VRCContactSender>();

            senderRootContact.shapeType = ContactBase.ShapeType.Sphere;
            senderRootContact.radius = 0.001f;
            senderRootContact.collisionTags.AddRange(new []
            {
                "TPS_Orf_Root",
                "TPS_Orf_Root_SelfNotOnHips",
                "SPSLL_Socket_Root",
                "SPSLL_Socket_Root_SelfNotOnHips",
                "SPSLL_Socket_Hole",
                "SPSLL_Socket_Hole_SelfNotOnHips"
            });

            senderFrontContact.shapeType = ContactBase.ShapeType.Sphere;
            senderFrontContact.radius = 0.001f;
            senderFrontContact.position = Vector3.forward * 0.01f;
            senderFrontContact.collisionTags.AddRange(new[]
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
        internal static void CreateHaptics(Transform root)
        {
            var hapticsRoot = Processor.CreateParentGameObject("Haptics", root);
            var penSelfNewRoot = Processor.CreateParentGameObject("PenSelfNewRoot", hapticsRoot.transform);
            var penSelfNewTip = Processor.CreateParentGameObject("PenSelfNewTip", hapticsRoot.transform);
            var penOthersNewRoot = Processor.CreateParentGameObject("PenOthersNewRoot", hapticsRoot.transform);
            var penOthersNewTip = Processor.CreateParentGameObject("PenOthersNewTip", hapticsRoot.transform);

            var receiverPenSelfNewRoot = penSelfNewRoot.AddComponent<VRCContactReceiver>();
            var receiverPenSelfNewTip = penSelfNewTip.AddComponent<VRCContactReceiver>();
            var receiverPenOthersNewRoot = penOthersNewRoot.AddComponent<VRCContactReceiver>();
            var receiverPenOthersNewTip = penOthersNewTip.AddComponent<VRCContactReceiver>();

            receiverPenSelfNewRoot.shapeType = ContactBase.ShapeType.Sphere;
            receiverPenSelfNewRoot.radius = 1f;
            receiverPenSelfNewRoot.position = Vector3.zero;
            receiverPenSelfNewRoot.rotation = Quaternion.identity;
            receiverPenSelfNewRoot.allowSelf = true;
            receiverPenSelfNewRoot.allowOthers = false;
            receiverPenSelfNewRoot.localOnly = true;
            receiverPenSelfNewRoot.collisionTags.AddRange(new[]
            {
                "TPS_Pen_Root"
            });
            receiverPenSelfNewRoot.receiverType = ContactReceiver.ReceiverType.Proximity;
            receiverPenSelfNewRoot.parameter = $"OGB/Orf/{root.gameObject.name}/{receiverPenSelfNewRoot.gameObject.name}";
            
            receiverPenSelfNewTip.shapeType = ContactBase.ShapeType.Sphere;
            receiverPenSelfNewTip.radius = 1f;
            receiverPenSelfNewTip.position = Vector3.zero;
            receiverPenSelfNewTip.rotation = Quaternion.identity;
            receiverPenSelfNewTip.allowSelf = true;
            receiverPenSelfNewTip.allowOthers = false;
            receiverPenSelfNewTip.localOnly = true;
            receiverPenSelfNewTip.collisionTags.AddRange(new[]
            {
                "TPS_Pen_Penetrating"
            });
            receiverPenSelfNewTip.receiverType = ContactReceiver.ReceiverType.Proximity;
            receiverPenSelfNewTip.parameter = $"OGB/Orf/{root.gameObject.name}/{receiverPenSelfNewTip.gameObject.name}";
            
            receiverPenOthersNewRoot.shapeType = ContactBase.ShapeType.Sphere;
            receiverPenOthersNewRoot.radius = 1f;
            receiverPenOthersNewRoot.position = Vector3.zero;
            receiverPenOthersNewRoot.rotation = Quaternion.identity;
            receiverPenOthersNewRoot.allowSelf = false;
            receiverPenOthersNewRoot.allowOthers = true;
            receiverPenOthersNewRoot.localOnly = true;
            receiverPenOthersNewRoot.collisionTags.AddRange(new[]
            {
                "TPS_Pen_Root"
            });
            receiverPenOthersNewRoot.receiverType = ContactReceiver.ReceiverType.Proximity;
            receiverPenOthersNewRoot.parameter = $"OGB/Orf/{root.gameObject.name}/{receiverPenOthersNewRoot.gameObject.name}";
            
            receiverPenOthersNewTip.shapeType = ContactBase.ShapeType.Sphere;
            receiverPenOthersNewTip.radius = 1f;
            receiverPenOthersNewTip.position = Vector3.zero;
            receiverPenOthersNewTip.rotation = Quaternion.identity;
            receiverPenOthersNewTip.allowSelf = false;
            receiverPenOthersNewTip.allowOthers = true;
            receiverPenOthersNewTip.localOnly = true;
            receiverPenOthersNewTip.collisionTags.AddRange(new[]
            {
                "TPS_Pen_Penetrating"
            });
            receiverPenOthersNewTip.receiverType = ContactReceiver.ReceiverType.Proximity;
            receiverPenOthersNewTip.parameter = $"OGB/Orf/{root.gameObject.name}/{receiverPenOthersNewTip.gameObject.name}";
        }
        
        /// <summary>
        /// Plugの位置によってアニメーションさせる奴
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAnimations(Transform root)
        {
            
        }
        
        /// <summary>
        /// Plugが接近したら自動でOnになる機能に使われてるっぽい
        /// </summary>
        /// <param name="root"></param>
        internal static void CreateAutoDistance(Transform root)
        {
            var autoDistanceRoot = Processor.CreateParentGameObject("AutoDistance", root);
            var receiverGameObject = Processor.CreateParentGameObject("Receiver", autoDistanceRoot.transform);
            
            var receiver = receiverGameObject.AddComponent<VRCContactReceiver>();
            
            receiver.shapeType = ContactBase.ShapeType.Sphere;
            receiver.radius = 0.3f;
            receiver.position = Vector3.zero;
            receiver.rotation = Quaternion.identity;
            receiver.allowSelf = false;
            receiver.allowOthers = true;
            receiver.localOnly = false;
            receiver.collisionTags.AddRange(new[]
            {
                "TPS_Pen_Penetrating"
            });
            receiver.receiverType = ContactReceiver.ReceiverType.Proximity;
            //TODO: 決まったパラメータはどこから来てるのか調べる。
            //receiver.parameter = $"VF94_Blowjob_Ring/{autoDistanceRoot.gameObject.name}";
        }
    }
}