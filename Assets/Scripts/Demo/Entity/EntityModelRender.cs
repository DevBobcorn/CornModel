#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using CraftSharp.Resource;

namespace CraftSharp.Demo
{
    public class EntityModelRender : MonoBehaviour
    {
        private static readonly char SP = Path.DirectorySeparatorChar;

        private EntityDefinition? entityDefinition = null;

        public Dictionary<string, GameObject> boneObjects = new();

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

        public void SetDefinitionData(EntityDefinition def) => entityDefinition = def;

        public void BuildEntityModel(string resourcePath, string geometryName, EntityGeometry geometry, Material defaultMaterial)
        {
            if (entityDefinition is null)
            {
                Debug.LogError("Entity definition not assigned!");
                return;
            }

            var texName = entityDefinition.TexturePaths.First().Value;
            var fileName = GetImagePathFromFileName(resourcePath + SP + texName);
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

                var boneMeshObj = new GameObject($"Mesh [{bone.Name}]");
                boneMeshObj.transform.SetParent(boneObj.transform, false);
                var boneMeshFilter = boneMeshObj.AddComponent<MeshFilter>();
                var boneMeshRenderer = boneMeshObj.AddComponent<MeshRenderer>();
                //var boneMeshFilter = boneObj.AddComponent<MeshFilter>();
                //var boneMeshRenderer = boneObj.AddComponent<MeshRenderer>();

                var visualBuffer = new EntityVertexBuffer();

                for (int i = 0;i < bone.Cubes.Length;i++)
                {
                    EntityCubeGeometry.Build(ref visualBuffer, geometry.TextureWidth, geometry.TextureHeight,
                            bone.MirrorUV, bone.Pivot, bone.Cubes[i]);
                }

                boneMeshFilter!.sharedMesh = EntityVertexBufferBuilder.BuildMesh(visualBuffer);
                boneMeshRenderer!.sharedMaterial = material;

                boneObjects.Add(bone.Name, boneObj);
            }

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
        }
    }
}