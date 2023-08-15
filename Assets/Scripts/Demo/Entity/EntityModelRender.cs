using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

using MinecraftClient.Resource;

namespace MinecraftClient.Demo
{
    public class EntityModelRender : MonoBehaviour
    {
        private static readonly char SP = Path.DirectorySeparatorChar;
        [SerializeField] private string bedrockResFolder;
        [SerializeField] private string entityDefPath;
        [SerializeField] private string entityGeoPath;
        [SerializeField] private Material defaultMaterial;

        public readonly Dictionary<string, string> TexturePaths = new();
        [HideInInspector] public string DefaultGeometryName = string.Empty;
        public Dictionary<string, EntityGeometry> Geometries = new();

        public Dictionary<string, GameObject> boneObjects = new();

        private string GetImagePathFromFileBaseName(string baseName)
        {
            var basePath = $"{bedrockResFolder}{SP}{baseName}";

            // Image could be either tga or png
            if (File.Exists($"{basePath}.png"))
            {
                return $"{basePath}.png";
            }
            else if (File.Exists($"{basePath}.tga"))
            {
                return $"{basePath}.tga";
            }

            // Nothing found
            return basePath;
        }

        private void LoadFromDefinition()
        {
            // Load entity definition
            var defJson = Json.ParseJson(File.ReadAllText($"{bedrockResFolder}{SP}{entityDefPath}"));
            var desc = defJson.Properties["minecraft:client_entity"].Properties["description"];
            TexturePaths.Clear();
            foreach (var pair in desc.Properties["textures"].Properties)
            {
                TexturePaths.Add(pair.Key, pair.Value.StringValue);
            }
            DefaultGeometryName = desc.Properties["geometry"].Properties["default"].StringValue;

            // Load entity geometries
            var geoJson = Json.ParseJson(File.ReadAllText($"{bedrockResFolder}{SP}{entityGeoPath}"));
            Geometries = EntityGeometry.TableFromJson(geoJson);
        }

        void BuildEntityModel(EntityGeometry geometry)
        {
            var texture = new Texture2D(2, 2);
            var texName = TexturePaths.First().Value;
            var fileName = GetImagePathFromFileBaseName(texName);
            // Load texture from file
            texture.LoadImage(File.ReadAllBytes(fileName));
            texture.filterMode = FilterMode.Point;
            Debug.Log($"Loaded texture from {fileName} ({texture.width}x{texture.height})");

            if (geometry.TextureWidth != texture.width && geometry.TextureHeight != texture.height)
            {
                Debug.LogWarning($"Specified texture size({geometry.TextureWidth}x{geometry.TextureHeight}) doesn't match image file!");
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

                //boneMeshObj.transform.localPosition = bone.Pivot / 16F;
                //boneMeshObj.transform.localRotation = Rotations.RotationFromEularsXYZ(bone.Rotation);

                var boneMeshFilter = boneMeshObj.AddComponent<MeshFilter>();
                var boneMeshRenderer = boneMeshObj.AddComponent<MeshRenderer>();

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
                    boneTransform.SetParent(boneObjects[bone.ParentName].transform, false);
                    boneTransform.localPosition = (bone.Pivot - geometry.Bones[bone.ParentName].Pivot) / 16F;
                    boneTransform.localRotation = Rotations.RotationFromEularsXYZ(bone.Rotation);
                }
                else // Root bone
                {
                    boneTransform.localPosition = bone.Pivot / 16F;
                    boneTransform.localRotation = Rotations.RotationFromEularsXYZ(bone.Rotation);
                }
            }
        }

        void Start()
        {
            // Load model data
            LoadFromDefinition();

            if (Geometries.ContainsKey(DefaultGeometryName))
            {
                // Build model meshes
                BuildEntityModel(Geometries[DefaultGeometryName]);
            }
            else
            {
                Debug.LogWarning($"No geometry named {DefaultGeometryName} could be found.");
            }
        }
    }
}