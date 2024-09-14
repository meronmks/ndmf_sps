using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;
using VRC.Dynamics;
using Object = UnityEngine.Object;

namespace com.meronmks.ndmfsps
{
    using runtime;
    using UnityEditor;
    using UnityEditor.Animations;
    
    internal static class PlugProcessor
    {
        private const string SENDER_PARAMPREFIX = "OGB/Pen/";
        
        internal static SpsSize GetWorldSize(Plug plug)
        {
            var renderers = GetRenderers(plug);
            Vector3 worldPosition = plug.transform.position;
            Quaternion worldRotation = plug.transform.rotation;

            if (plug.detectTransform4Mesh && renderers.Count > 0)
            {
                worldPosition = Processor.GetMeshRoot(renderers.First()).position;
                var localRotation = Processor.GetMaterialDpsRotation(renderers.First()) ?? Quaternion.identity;
                worldRotation = Processor.GetMeshRoot(renderers.First()).rotation * localRotation;
            }

            var ogbTestBase = plug.transform.Find("OGBTestBase");
            if (ogbTestBase != null)
            {
                worldPosition = ogbTestBase.position;
                worldRotation = ogbTestBase.rotation;
            }
            
            float worldLength = 0;
            float worldRadius = 0;

            MultiMapHashSet<GameObject, int> matSlots = new MultiMapHashSet<GameObject, int>();
            if (plug.detectLength || plug.detectRadius || renderers.Count > 0)
            {
                if (renderers.Count == 0)
                {
                    throw new Exception("Failed to find plug renderer");
                }

                var size = GetAutoWorldSize(renderers, worldPosition, worldRotation, plug);
                if (size != null)
                {
                    if (plug.detectLength)
                    {
                        worldLength = size.Item1;
                    }

                    if (plug.detectRadius)
                    {
                        worldRadius = size.Item2;
                    }
                    matSlots = size.Item3;
                }
            }

            if (!plug.detectLength)
            {
                worldLength = plug.length;
                if (!plug.unitsInMeters)
                {
                    worldLength *= plug.transform.lossyScale.x;
                }
            }
            
            if (!plug.detectRadius)
            {
                worldRadius = plug.radius;
                if (!plug.unitsInMeters)
                {
                    worldRadius *= plug.transform.lossyScale.x;
                }
            }
            
            return new SpsSize
            {
                renderers = renderers,
                worldLength = worldLength,
                worldRadius = worldRadius,
                localPosition = plug.transform.InverseTransformPoint(worldPosition),
                localRotation = Quaternion.Inverse(plug.transform.rotation) * worldRotation,
                matSlots = matSlots
            };
        }

        internal static IImmutableList<Renderer> GetAutoRenderer(Transform transform)
        {
            var currentTF = transform;

            while (currentTF != null)
            {
                var foundObject = currentTF.GetComponents<Renderer>().ToImmutableList();
                if (foundObject.Count == 1)
                {
                    return foundObject;
                } else if (foundObject.Count > 1)
                {
                    return ImmutableList<Renderer>.Empty;
                }
                
                currentTF = currentTF.parent;
            }
            
            return ImmutableList<Renderer>.Empty;
        }

        internal static ICollection<Renderer> GetRenderers(Plug plug)
        {
            var renderers = new List<Renderer>();
            if (plug.automaticallyFindMesh)
            {
                renderers.AddRange(GetAutoRenderer(plug.transform));
            }
            else
            {
                renderers.AddRange(plug.meshRenderers);
            }

            return renderers;
        }
        
        internal static GameObject CreateBakedSpsPlug(Plug plug)
        {
            return Processor.CreateParentGameObject("BakedSpsPlug", plug.transform);
        }

        internal static void CreateSenders(GameObject root, Plug plug, SpsSize size, Animator animator)
        {
            var senders = Processor.CreateParentGameObject("Senders", root.transform);
            var lengthGO = Processor.CreateParentGameObject("Length", senders.transform);
            Processor.CreateVRCContactSender(
                lengthGO,
                size.worldLength,
                Vector3.zero,
                Quaternion.identity,
                new []
                {
                    "TPS_Pen_Penetrating"
                },
                animator,
                useHipAvoidance: plug.useHipAvoidance);
            
            var widthHelperGO = Processor.CreateParentGameObject("WidthHelper", senders.transform);
            Processor.CreateVRCContactSender(
                widthHelperGO,
                Mathf.Max(0.01f, size.worldLength - size.worldRadius*2),
                Vector3.zero,
                Quaternion.identity,
                new []
                {
                    "TPS_Pen_Width"
                },
                animator,
                useHipAvoidance: plug.useHipAvoidance);
            
            var envelopeGO = Processor.CreateParentGameObject("Envelope", senders.transform);
            Processor.CreateVRCContactSender(
                envelopeGO,
                size.worldRadius,
                Vector3.forward * (size.worldLength / 2),
                Quaternion.Euler(90,0,0),
                new []
                {
                    "TPS_Pen_Close"
                },
                animator,
                height: size.worldLength,
                useHipAvoidance: plug.useHipAvoidance);
            
            var rootGO = Processor.CreateParentGameObject("Root", senders.transform);
            Processor.CreateVRCContactSender(
                rootGO,
                0.01f,
                Vector3.zero,
                Quaternion.identity,
                new []
                {
                    "TPS_Pen_Root"
                },
                animator,
                useHipAvoidance: plug.useHipAvoidance);
        }

        internal static void CreateHaptic(GameObject root, Plug plug, SpsSize size, Animator animator)
        {
            var hapticsRoot = Processor.CreateParentGameObject("Haptics", root.transform);
            var extraRadiusForTouch = Math.Min(size.worldRadius, 0.08f);
            
            var touchSelfCloseGO = Processor.CreateParentGameObject("TouchSelfClose", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                touchSelfCloseGO,
                size.worldRadius+extraRadiusForTouch,
                Vector3.forward * (size.worldLength / 2),
                true,
                Processor.selfContacts,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelfCloseGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self,
                receiverType: ContactReceiver.ReceiverType.Constant,
                height: size.worldLength+extraRadiusForTouch*2,
                rot: Quaternion.Euler(90,0,0),
                useHipAvoidance: plug.useHipAvoidance);
            
            var touchSelfGO = Processor.CreateParentGameObject("TouchSelf", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                touchSelfGO,
                size.worldRadius+extraRadiusForTouch,
                Vector3.zero, 
                true,
                Processor.selfContacts,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchSelfGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self,
                useHipAvoidance: plug.useHipAvoidance);
            
            var touchOthersCloseGO = Processor.CreateParentGameObject("TouchOthersClose", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                touchOthersCloseGO,
                size.worldRadius+extraRadiusForTouch,
                Vector3.forward * (size.worldLength / 2),
                true,
                Processor.bodyContacts,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthersCloseGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                receiverType: ContactReceiver.ReceiverType.Constant,
                height: size.worldLength+extraRadiusForTouch*2,
                useHipAvoidance: plug.useHipAvoidance);
            
            var touchOthersGO = Processor.CreateParentGameObject("TouchOthers", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                touchOthersGO,
                size.worldRadius+extraRadiusForTouch,
                Vector3.zero,
                true,
                Processor.bodyContacts,
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{touchOthersGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: plug.useHipAvoidance);
            
            var penSelfGO = Processor.CreateParentGameObject("PenSelf", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                penSelfGO,
                size.worldLength,
                Vector3.zero,
                true,
                new []
                {
                    "TPS_Orf_Root"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penSelfGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Self,
                useHipAvoidance: plug.useHipAvoidance);
            
            var penOthersGO = Processor.CreateParentGameObject("PenOthers", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                penOthersGO,
                size.worldLength,
                Vector3.zero,
                true,
                new []
                {
                    "TPS_Orf_Root"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{penOthersGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: plug.useHipAvoidance);
            
            var frotOthersGO = Processor.CreateParentGameObject("FrotOthers", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                frotOthersGO,
                size.worldLength,
                Vector3.zero,
                true,
                new []
                {
                    "TPS_Pen_Close"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{frotOthersGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: plug.useHipAvoidance);
            
            var frotOthersCloseGO = Processor.CreateParentGameObject("FrotOthersClose", hapticsRoot.transform);
            Processor.CreateVRCContactReceiver(
                frotOthersCloseGO,
                size.worldLength,
                Vector3.forward * (size.worldLength / 2),
                true,
                new []
                {
                    "TPS_Pen_Close"
                },
                $"{SENDER_PARAMPREFIX}{root.gameObject.name.Replace("/", "_")}/{frotOthersCloseGO.name.Replace("/", "_")}",
                animator,
                Processor.ReceiverParty.Others,
                height: size.worldLength,
                rot: Quaternion.Euler(90,0,0),
                receiverType: ContactReceiver.ReceiverType.Constant,
                useHipAvoidance: plug.useHipAvoidance);
        }

        internal static void CreateSpsPlus(GameObject root, Plug plug, SpsSize size, Animator animator)
        {
            var spsPlusRoot = Processor.CreateParentGameObject("SpsPlus", root.transform);

            const string ringTag = "SPSLL_Socket_Ring";
            
            var spsllSocketRingSelfGo = Processor.CreateParentGameObject($"{ringTag}Self", spsPlusRoot.transform);
            Processor.CreateVRCContactReceiver(
                spsllSocketRingSelfGo,
                3f,
                Vector3.zero,
                false,
                new [] {ringTag},
                $"spsll_{ringTag}_self",
                animator,
                Processor.ReceiverParty.Self,
                useHipAvoidance: plug.useHipAvoidance);
            
            var spsllSocketRingOtherGo = Processor.CreateParentGameObject($"{ringTag}Others", spsPlusRoot.transform);
            Processor.CreateVRCContactReceiver(
                spsllSocketRingOtherGo,
                3f,
                Vector3.zero,
                false,
                new [] {ringTag},
                $"spsll_{ringTag}_others",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: plug.useHipAvoidance);
            
            const string holeTag = "SPSLL_Socket_Hole";
            
            var spsllSocketHoleSelfGo = Processor.CreateParentGameObject($"{holeTag}Self", spsPlusRoot.transform);
            Processor.CreateVRCContactReceiver(
                spsllSocketHoleSelfGo,
                3f,
                Vector3.zero,
                false,
                new [] {holeTag},
                $"spsll_{holeTag}_self",
                animator,
                Processor.ReceiverParty.Self,
                useHipAvoidance: plug.useHipAvoidance);
            
            var spsllSocketHoleOtherGo = Processor.CreateParentGameObject($"{holeTag}Others", spsPlusRoot.transform);
            Processor.CreateVRCContactReceiver(
                spsllSocketHoleOtherGo,
                3f,
                Vector3.zero,
                false,
                new [] {holeTag},
                $"spsll_{holeTag}_others",
                animator,
                Processor.ReceiverParty.Others,
                useHipAvoidance: plug.useHipAvoidance);
        }

        internal static SkinnedMeshRenderer CreateNormalizeRenderer(Renderer renderer, GameObject root, float worldLength)
        {
            if (renderer is MeshRenderer) {
                var obj = renderer.gameObject;
                var staticMesh = GetMesh(renderer);
                var meshFilter = obj.GetComponent<MeshFilter>();
                var mats = renderer.sharedMaterials;
                var anchor = renderer.probeAnchor;

                Object.DestroyImmediate(renderer);
                Object.DestroyImmediate(meshFilter);

                var newSkin = obj.AddComponent<SkinnedMeshRenderer>();
                newSkin.sharedMesh = staticMesh;
                newSkin.sharedMaterials = mats;
                newSkin.probeAnchor = anchor;
                renderer = newSkin;
            }

            var skin = renderer as SkinnedMeshRenderer;
            if (skin == null)
            {
                NDMFConsole.LogError("ndmf.console.plug.unknowRendererType");
                throw new Exception("Unknown renderer type");
            }
            var mesh = GetMesh(skin);
            if (mesh == null)
            {
                NDMFConsole.LogError("ndmf.console.plug.missingMesh");
                throw new Exception("Missing mesh");
            }

            if (mesh.boneWeights.Length == 0)
            {
                var spsMainBoneGO = Processor.CreateParentGameObject("SpsMainBone", skin.transform);
                var meshCopy = CopyMesh(mesh);
                meshCopy.boneWeights = meshCopy.vertices.Select(v => new BoneWeight { weight0 = 1 }).ToArray();
                meshCopy.bindposes = new[]
                {
                    Matrix4x4.identity,
                };
                skin.bones = new Transform[] { spsMainBoneGO.transform };
                skin.sharedMesh = meshCopy;
                mesh = meshCopy;
            }

            skin.rootBone = root.transform;

            var bakeVertices = GetMeshVertices(skin, skin.rootBone);
            var bounds = new Bounds();
            foreach (var vertex in bakeVertices)
            {
                bounds.Encapsulate(vertex);
            }
            
            var localLength = worldLength / root.transform.lossyScale.z;
            bounds.Encapsulate(new Vector3(localLength * 2f,localLength * 2f,localLength * 2.5f));
            bounds.Encapsulate(new Vector3(localLength * -2f,localLength * -2f,localLength * 2.5f));
            bounds.Encapsulate(new Vector3(localLength * 2f,localLength * 2f,localLength * -0.5f));
            bounds.Encapsulate(new Vector3(localLength * -2f,localLength * -2f,localLength * -0.5f));
            skin.localBounds = bounds;
            skin.updateWhenOffscreen = false;

            return skin;
        }

        internal static Tuple<float, float, MultiMapHashSet<GameObject, int>> GetAutoWorldSize(
            ICollection<Renderer> renderers, Vector3 worldPos, Quaternion worldRot, Plug plug = null)
        {
            if (renderers.Count == 0) return null;
            var inverseWorldRot = Quaternion.Inverse(worldRot);

            var allLocalVec = new List<Vector3>();
            var matsUsed = new MultiMapHashSet<GameObject, int>();

            foreach (var renderer in renderers)
            {
                var meshVertices = GetMeshVertices(renderer);
                if (meshVertices == null) continue;
                float[] mask = plug ? GetMask(renderer, plug) : null;
                var matsUsedVert = new MultiMapHashSet<int, int>();
                var mesh = GetMesh(renderer);
                var matSlotsInMesh = 0;
                if (mesh != null)
                {
                    var matCount = matSlotsInMesh = mesh.subMeshCount;
                    for (var matI = 0; matI < matCount; matI++)
                    {
                        foreach (var vert in mesh.GetTriangles(matI))
                        {
                            matsUsedVert.Put(vert, matI);
                        }
                    }
                }

                var localVerts = meshVertices
                    .Select(vert => renderer.transform.TransformPoint(vert))
                    .Select(v => inverseWorldRot * (v - worldPos))
                    .Select((v, i) =>
                    {
                        var isUsed = mask == null || mask[i] > 0;
                        isUsed &= v.z > 0;
                        return (v, i, isUsed);
                    });
                foreach (var (v, index, isUsed) in localVerts)
                {
                    if (!isUsed) continue;
                    foreach (var matI in matsUsedVert.Get(index))
                    {
                        matsUsed.Put(renderer.gameObject, matI);
                    }
                    allLocalVec.Add(v);
                }

                if (matsUsed.Get(renderer.gameObject).Contains(matSlotsInMesh - 1))
                {
                    for (var i = matSlotsInMesh; i < renderer.sharedMaterials.Length; i++)
                    {
                        matsUsed.Put(renderer.gameObject, i);
                    }
                }
            }

            var lenght = allLocalVec
                .Select(v => v.z)
                .DefaultIfEmpty(0)
                .Max();
            var radius = allLocalVec
                .Select(v => Vector3.Cross(v, Vector3.forward).magnitude)
                .OrderBy(m => m)
                .Where((m, i) => i <= allLocalVec.Count * 0.75)
                .DefaultIfEmpty(0)
                .Max();

            if (lenght <= 0 || radius <= 0) return null;
            
            return Tuple.Create(lenght, radius, matsUsed);
        }

        private static Vector3[] GetMeshVertices(Renderer renderer, Transform origin = null)
        {
            var mesh = GetMesh(renderer);

            if (renderer is SkinnedMeshRenderer skin)
            {
                var tempMesh = new Mesh();
                skin.BakeMesh(tempMesh);

                var actuallySkinned = mesh != null && mesh.boneWeights.Length > 0;
                if (actuallySkinned)
                {
                    var scale = skin.transform.lossyScale;
                    var inverseScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);
                    ApplyScale(tempMesh, inverseScale);
                }
                
                mesh = tempMesh;
            }

            if (mesh == null) return null;
            
            Vector3[] vertices = mesh.vertices;

            if (origin != null)
            {
                vertices = vertices.Select(v => origin.InverseTransformPoint(renderer.transform.TransformPoint(v))).ToArray();
            }
            
            return vertices;
        }

        private static Mesh GetMesh(Renderer renderer)
        {
            Mesh mesh = null;

            if (renderer is SkinnedMeshRenderer skin)
            {
                if (skin.sharedMesh == null) return null;
                mesh = skin.sharedMesh;
            }else if (renderer is MeshRenderer)
            {
                var filter = renderer.gameObject.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) return null;
                mesh = filter.sharedMesh;
            }

            if (mesh != null && !mesh.isReadable && Application.isPlaying)
            {
                return CopyMesh(mesh);
            }

            return mesh;
        }

        private static Mesh CopyMesh(Mesh mesh)
        {
            Mesh m = new Mesh ();

            m.vertices = mesh.vertices;
            m.uv = mesh.uv;
            m.uv2 = mesh.uv2;
            m.uv3 = mesh.uv3;
            m.uv4 = mesh.uv4;
            m.triangles = mesh.triangles;

            m.bindposes = mesh.bindposes;
            m.boneWeights = mesh.boneWeights;
            m.bounds = mesh.bounds;
            m.colors = mesh.colors;
            m.colors32 = mesh.colors32;
            m.normals = mesh.normals;
            m.subMeshCount = mesh.subMeshCount;
            m.tangents = mesh.tangents;

            return m;
        }
        
        private static void ApplyScale(Mesh mesh, Vector3 scale) {
            var verts = mesh.vertices;
            ApplyScale(verts, scale);
            mesh.vertices = verts;
        }
        private static void ApplyScale(Vector3[] verts, Vector3 scale) {
            for (var i = 0; i < verts.Length; i++) {
                verts[i].Scale(scale);
            }
        }

        private static float[] GetMask(Renderer renderer, Plug plug)
        {
            var mesh = GetMesh(renderer);
            if (mesh == null) return null;
            
            var textureMask = MakeReadable(plug.textureMask);

            ISet<int> includedBoneIds = ImmutableHashSet<int>.Empty;
            if (plug.automaticallyMaskUsingBoneWeights && renderer is SkinnedMeshRenderer skin)
            {
                var firstBone = plug.transform;
                while (firstBone != null)
                {
                    if (skin.bones.Contains(firstBone))
                    {
                        break;
                    }

                    firstBone = firstBone.parent;
                }

                if (firstBone != null)
                {
                    includedBoneIds = firstBone.gameObject
                        .GetComponentsInChildren<Transform>(true)
                        .ToArray()
                        .Select(bone => Array.IndexOf(skin.bones, bone))
                        .Where(id => id >= 0)
                        .ToImmutableHashSet();
                }
            }

            float[] output = new float[mesh.vertices.Length];
            for (var i = 0; i < mesh.vertices.Length; i++)
            {
                var weight = 1f;
                if (mesh.boneWeights.Length > 0)
                {
                    weight = GetWeight(mesh.boneWeights[i], includedBoneIds);
                }

                var texture = 1f;
                if (textureMask != null)
                {
                    var pC = textureMask.GetPixelBilinear(mesh.uv[i].x, mesh.uv[i].y);
                    texture = 1 - Math.Min(pC.maxColorComponent, pC.a);
                }

                output[i] = Math.Min(weight, texture);
            }

            return output;
        }
        
        private static float GetWeight(BoneWeight boneWeight, ICollection<int> boneIds) {
            if (boneIds.Count == 0) return 1;
            var weightedToBone = 0f;
            if (boneIds.Contains(boneWeight.boneIndex0)) weightedToBone += boneWeight.weight0;
            if (boneIds.Contains(boneWeight.boneIndex1)) weightedToBone += boneWeight.weight1;
            if (boneIds.Contains(boneWeight.boneIndex2)) weightedToBone += boneWeight.weight2;
            if (boneIds.Contains(boneWeight.boneIndex3)) weightedToBone += boneWeight.weight3;
            var totalWeight = boneWeight.weight0 + boneWeight.weight1 + boneWeight.weight2 + boneWeight.weight3;
            if (totalWeight > 0) {
                weightedToBone /= totalWeight;
            }
            return weightedToBone;
        }
        
        private static Texture2D MakeReadable(Texture2D texture) {
            if (texture == null) return null;
            if (texture.isReadable) return texture;
            var tmp = RenderTexture.GetTemporary(
                texture.width,
                texture.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            Graphics.Blit(texture, tmp);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = tmp;
            Texture2D myTexture2D = new Texture2D(texture.width, texture.height);
            myTexture2D.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
            myTexture2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(tmp);
            return myTexture2D;
        }
    }
}