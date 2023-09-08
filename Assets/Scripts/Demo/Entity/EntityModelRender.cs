#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Unity.Mathematics;

using CraftSharp.Resource;
using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Demo
{
    public class EntityModelRender : MonoBehaviour
    {
        private EntityDefinition? entityDefinition = null;

        public Dictionary<string, GameObject> boneObjects = new();
        public Dictionary<string, GameObject> boneMeshObjects = new();

        private EntityGeometry? geometry = null;

        public string[] animationNames = { };
        private EntityAnimation?[] animations = { };
        private EntityAnimation? currentAnimation = null;

        private MoScope scope = new(new MoLangRuntime());
        private MoLangEnvironment env = new();

        private string GetImagePathFromFileName(string name)
        {
            // Image could be either tga or png
            if (File.Exists($"{name}.png"))
            {
                return $"{name}.png";
            }
            else if (File.Exists($"{name}.tga"))
            {
                return $"{name}.tga";
            }

            // Nothing found
            return name;
        }

        public void SetDefinitionData(EntityDefinition def)
        {
            entityDefinition = def;
        }

        public void BuildEntityModel(EntityResourceManager entityResManager, Material defaultMaterial)
        {
            if (entityDefinition is null)
            {
                Debug.LogError("Entity definition not assigned!");
                return;
            }

            if (entityDefinition.GeometryNames.Count == 0)
            {
                Debug.LogWarning($"Entity definition has no geometry!");
                return;
            }

            var geometryName = entityDefinition.GeometryNames.First().Value;
            gameObject.name += $" ({geometryName})";

            if (!entityResManager.entityGeometries.ContainsKey(geometryName))
            {
                Debug.LogWarning($"Entity geometry [{geometryName}] not loaded!");
                return;
            }

            geometry = entityResManager.entityGeometries[geometryName];

            var texName = entityDefinition.TexturePaths.First().Value;
            var fileName = GetImagePathFromFileName(texName);
            // Load texture from file
            Texture2D texture;
            var imageBytes = File.ReadAllBytes(fileName);
            if (fileName.EndsWith(".tga")) // Read as tga image
            {
                texture = TGALoader.TextureFromTGA(imageBytes);
            }
            else // Read as png image
            {
                texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
            }

            texture.filterMode = FilterMode.Point;
            //Debug.Log($"Loaded texture from {fileName} ({texture.width}x{texture.height})");

            if (geometry.TextureWidth != texture.width && geometry.TextureHeight != texture.height)
            {
                if (geometry.TextureWidth == 0 && geometry.TextureHeight == 0) // Not specified, just use the size we have
                {
                    geometry.TextureWidth = texture.width;
                    geometry.TextureHeight = texture.height;
                }
                else // The sizes doesn't match
                {
                    Debug.LogWarning($"Specified texture size({geometry.TextureWidth}x{geometry.TextureHeight}) doesn't match image file {texName} ({texture.width}x{texture.height})!");
                }
            }

            // Make a copy of the material
            var material = new Material(defaultMaterial) { mainTexture = texture };

            // Build mesh for each bone
            foreach (var bone in geometry.Bones.Values)
            {
                var boneObj = new GameObject($"Bone [{bone.Name}]");
                boneObj.transform.SetParent(transform, false);

                //var boneMeshObj = new GameObject($"Mesh [{bone.Name}]");
                //boneMeshObj.transform.SetParent(boneObj.transform, false);
                var boneMeshFilter = boneObj.AddComponent<MeshFilter>();
                var boneMeshRenderer = boneObj.AddComponent<MeshRenderer>();

                var visualBuffer = new EntityVertexBuffer();

                for (int i = 0;i < bone.Cubes.Length;i++)
                {
                    EntityCubeGeometry.Build(ref visualBuffer, geometry.TextureWidth, geometry.TextureHeight,
                            bone.MirrorUV, bone.Pivot, bone.Cubes[i]);
                }

                boneMeshFilter!.sharedMesh = EntityVertexBufferBuilder.BuildMesh(visualBuffer);
                boneMeshRenderer!.sharedMaterial = material;

                boneObjects.Add(bone.Name, boneObj);
                boneMeshObjects.Add(bone.Name, boneObj);
            }
            // Setup initial bone pose
            foreach (var bone in geometry.Bones.Values)
            {
                var boneTransform = boneObjects[bone.Name].transform;

                if (bone.ParentName is not null) // Set parent transform
                {
                    if (boneObjects.ContainsKey(bone.ParentName))
                    {
                        boneTransform.SetParent(boneObjects[bone.ParentName].transform, false);
                        boneTransform.localPosition = (bone.Pivot - geometry.Bones[bone.ParentName].Pivot) / 16F;
                        boneTransform.localRotation = Rotations.RotationFromEularsXYZ(bone.Rotation);
                    }
                    else
                    {
                        Debug.LogWarning($"In {geometryName}: parent bone {bone.ParentName} not found!");
                    }
                }
                else // Root bone
                {
                    boneTransform.localPosition = bone.Pivot / 16F;
                    boneTransform.localRotation = Rotations.RotationFromEularsXYZ(bone.Rotation);
                }
            }
        
            // Prepare animations
            animationNames = entityDefinition.AnimationNames.Select(x => $"{x.Key} ({x.Value})").ToArray();

            animations = entityDefinition.AnimationNames.Select(x => 
                    {
                        EntityAnimation? anim;

                        if (entityResManager.entityAnimations.ContainsKey(x.Value))
                        {
                            anim = entityResManager.entityAnimations[x.Value];
                        }
                        else
                        {
                            anim = null;
                            // TODO: Debug.LogWarning($"Animation [{x.Value}] not loaded!");
                        }

                        return anim; 
                    }).ToArray();
        }

        public EntityAnimation? SetAnimation(int index, float initialTime)
        {
            if (index >= 0 && index < animations.Length)
            {
                currentAnimation = animations[index];

                UpdateAnimation(initialTime);

                return currentAnimation;
            }
            else
            {
                throw new System.Exception($"Invalid animation index: {index}");
            }
        }

        public void UpdateAnimation(float time)
        {
            if (currentAnimation != null && geometry != null) // An animation file is present
            {
                foreach (var boneAnim in currentAnimation.BoneAnimations)
                {
                    if (boneObjects.ContainsKey(boneAnim.Key))
                    {
                        var (trans, scale, rot) = boneAnim.Value.Evaluate(time, scope, env);
                        UpdateBone(boneAnim.Key, trans, scale, rot);
                    }
                    else
                    {
                        Debug.Log($"Trying to update bone [{boneAnim.Key}] which is not present!");
                    }
                }
            }
        }

        public void UpdateMolangValue(MoPath varName, IMoValue value)
        {
            env.SetValue(varName, value);
        }

        private void UpdateBone(string boneName, float3? trans, float3? scale, float3? rot)
        {
            var boneTransform = boneObjects[boneName].transform;
            //var boneMeshTransform = boneMeshObjects[boneName].transform;

            var bone = geometry!.Bones[boneName];

            if (trans is not null)
            {
                var converted = trans.Value.zyx;
                converted.z = -converted.z;

                float3 offset;

                if (bone.ParentName is not null)
                    offset = (converted + bone.Pivot - geometry.Bones[bone.ParentName].Pivot) / 16F;
                else
                    offset = (converted + bone.Pivot) / 16F;

                boneTransform.localPosition = offset;
            }
            
            if (rot is not null)
            {
                var converted = rot.Value.zyx;
                converted.x = -converted.x;
                boneTransform.localRotation = Rotations.RotationFromEularsXYZ(converted);
            }
        }
    }
}