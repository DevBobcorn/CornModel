using System.Collections.Generic;
using UnityEngine;

using MinecraftClient.Resource;

namespace MinecraftClient.Rendering
{
    public static class MaterialManager {
        private static Dictionary<RenderType, Material> blockMaterials = new();

        private static Material solid;

        private static bool initialized = false;

        public static Material GetAtlasMaterial(RenderType renderType)
        {
            EnsureInitialized();
            return blockMaterials.GetValueOrDefault(renderType, solid);
        }

        public static void EnsureInitialized()
        {
            if (!initialized) Initialize();
        }

        private static void Initialize()
        {
            blockMaterials.Clear();

            // Solid
            var s = Resources.Load<Material>("Materials/Block/Block Solid");
            s.SetTexture("_BaseMap", AtlasManager.GetAtlasTexture(RenderType.SOLID));
            blockMaterials.Add(RenderType.SOLID, s);

            // Cutout & Cutout Mipped
            var c = Resources.Load<Material>("Materials/Block/Block Cutout");
            c.SetTexture("_BaseMap", AtlasManager.GetAtlasTexture(RenderType.CUTOUT));
            blockMaterials.Add(RenderType.CUTOUT, c);

            var cm = Resources.Load<Material>("Materials/Block/Block Cutout Mipped");
            cm.SetTexture("_BaseMap", AtlasManager.GetAtlasTexture(RenderType.CUTOUT_MIPPED));
            blockMaterials.Add(RenderType.CUTOUT_MIPPED, cm);

            // Translucent
            var t = Resources.Load<Material>("Materials/Block/Block Transparent");
            t.SetTexture("_BaseMap", AtlasManager.GetAtlasTexture(RenderType.TRANSLUCENT));
            blockMaterials.Add(RenderType.TRANSLUCENT, t);

            initialized = true;

        }

    }

}