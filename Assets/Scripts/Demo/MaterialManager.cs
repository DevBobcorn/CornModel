#nullable enable
using System.Collections.Generic;
using UnityEngine;

using CraftSharp.Resource;

namespace CraftSharp
{
    public class MaterialManager : MonoBehaviour
    {
        [SerializeField] public Material? AtlasSolid;
        [SerializeField] public Material? AtlasCutout;
        [SerializeField] public Material? AtlasCutoutMipped;
        [SerializeField] public Material? AtlasTranslucent;
        [SerializeField] public Material? AtlasWater;

        [SerializeField] public Material? PlayerSkin;

        [SerializeField] public Material? EntityDefault;

        private Dictionary<RenderType, Material> atlasMaterials = new();
        private Material? defaultAtlasMaterial;
        private Dictionary<string, Texture2D> skinTextures = new(); // First assign a place holder...
        private Dictionary<string, Material> skinMaterials = new();
        public Dictionary<string, Material> SkinMaterials => skinMaterials;

        private bool initialized = false;

        public Material GetAtlasMaterial(RenderType renderType)
        {
            EnsureInitialized();
            return atlasMaterials.GetValueOrDefault(renderType, defaultAtlasMaterial!);
        }

        public void EnsureInitialized()
        {
            if (!initialized) Initialize();
        }

        private void Initialize()
        {
            atlasMaterials.Clear();
            var packManager = ResourcePackManager.Instance;

            // Solid
            var solid = new Material(AtlasSolid!);
            solid.SetTexture("_BaseMap", packManager.GetAtlasArray(RenderType.SOLID));
            atlasMaterials.Add(RenderType.SOLID, solid);

            defaultAtlasMaterial = solid;

            // Cutout & Cutout Mipped
            var cutout = new Material(AtlasCutout!);
            cutout.SetTexture("_BaseMap", packManager.GetAtlasArray(RenderType.CUTOUT));
            atlasMaterials.Add(RenderType.CUTOUT, cutout);

            var cutoutMipped = new Material(AtlasCutoutMipped!);
            cutoutMipped.SetTexture("_BaseMap", packManager.GetAtlasArray(RenderType.CUTOUT_MIPPED));
            atlasMaterials.Add(RenderType.CUTOUT_MIPPED, cutoutMipped);

            // Translucent
            var translucent = new Material(AtlasTranslucent!);
            translucent.SetTexture("_BaseMap", packManager.GetAtlasArray(RenderType.TRANSLUCENT));
            atlasMaterials.Add(RenderType.TRANSLUCENT, translucent);

            // Water
            atlasMaterials.Add(RenderType.WATER, translucent);

            initialized = true;
        }
    }
}