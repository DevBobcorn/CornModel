using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MinecraftClient.Resource
{
    public class ResourcePackManager
    {
        public static string GetRootDirectory()
        {
            return Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/');
        }

        public static string GetPacksDirectory()
        {
            return Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/') + "/Resource Packs";
        }

        public static string GetPackDirectoryNamed(string packName)
        {
            return Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/') + "/Resource Packs/" + packName;
        }

        // Identifier -> Texture path
        public readonly Dictionary<ResourceLocation, string> textureTable = new Dictionary<ResourceLocation, string>();

        // Identifier -> Block model
        public readonly Dictionary<ResourceLocation, BlockModel> modelsTable = new Dictionary<ResourceLocation, BlockModel>();

        // Block state numeral id -> Block state geometries (One single block state may have a list of models to use randomly)
        public readonly Dictionary<int, List<BlockGeometry>> finalTable = new Dictionary<int, List<BlockGeometry>>();

        public readonly BlockModelLoader blockModelLoader;
        public readonly BlockStateModelLoader stateModelLoader;

        public readonly List<ResourcePack> packs = new List<ResourcePack>();

        public ResourcePackManager()
        {
            blockModelLoader = new BlockModelLoader(this);
            stateModelLoader = new BlockStateModelLoader(this);
        }

        public void AddPack(ResourcePack pack)
        {
            packs.Add(pack);
        }

        public void ClearPacks()
        {
            packs.Clear();
        }

        public void LoadPacks()
        {
            float startTime = Time.realtimeSinceStartup;

            foreach (var pack in packs)
            {
                if (pack.IsValid)
                {
                    pack.LoadResources(this);
                }
                
            }

            foreach (var pack in packs)
            {
                if (pack.IsValid)
                {
                    pack.BuildStateGeometries(this);
                }
                
            }

            Debug.Log("Resource packs loaded in " + (Time.realtimeSinceStartup - startTime) + " seconds.");
            Debug.Log("Built " + finalTable.Count + " block state geometry lists.");

        }


    }
}