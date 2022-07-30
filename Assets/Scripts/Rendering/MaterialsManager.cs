using System.Collections.Generic;
using UnityEngine;

using MinecraftClient.Resource;

namespace MinecraftClient.Rendering
{
    public class MaterialsManager {
        private static Material solidMaterial, cutoutMaterial, translucentMaterial;
        private static Dictionary<RenderType, Material> blockMaterials = new Dictionary<RenderType, Material>();

        private static bool initialized = false;

        public static Material GetBlockMaterial(RenderType renderType)
        {
            EnsureInitialized();
            return blockMaterials.GetValueOrDefault(renderType, cutoutMaterial);
        }

        public static void EnsureInitialized()
        {
            if (!initialized) Initialize();
        }

        private static void Initialize()
        {
            // Solid
            solidMaterial = new Material(Shader.Find("Unicorn/BlockSolid"));
            solidMaterial.SetTexture("_MainTex", BlockTextureManager.AtlasTexture);
            solidMaterial.SetFloat("_Glossiness", 0F);

            blockMaterials.Add(RenderType.SOLID, solidMaterial);

            // Cutout
            cutoutMaterial = new Material(Shader.Find("Unicorn/BlockCutout"));
            cutoutMaterial.EnableKeyword("_ALPHATEST_ON");
            cutoutMaterial.SetTexture("_MainTex", BlockTextureManager.AtlasTexture);
            cutoutMaterial.SetFloat("_Mode", 1);
            cutoutMaterial.renderQueue = 2450;
            cutoutMaterial.SetFloat("_Glossiness", 0F);
            cutoutMaterial.SetFloat("_Cutoff", 0.75F);

            blockMaterials.Add(RenderType.CUTOUT, cutoutMaterial);
            blockMaterials.Add(RenderType.CUTOUT_MIPPED, cutoutMaterial);

            // TODO Translucent
            translucentMaterial = new Material(Shader.Find("Unicorn/BlockCutout"));
            translucentMaterial.EnableKeyword("_ALPHATEST_ON");
            translucentMaterial.SetTexture("_MainTex", BlockTextureManager.AtlasTexture);
            translucentMaterial.SetFloat("_Mode", 1);
            translucentMaterial.renderQueue = 2450;
            translucentMaterial.SetFloat("_Glossiness", 0F);
            translucentMaterial.SetFloat("_Cutoff", 0.75F);

            blockMaterials.Add(RenderType.TRANSLUCENT, translucentMaterial);

            initialized = true;

        }

    }

}