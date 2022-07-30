using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using MinecraftClient.Mapping;
using MinecraftClient.Rendering;

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
        public readonly Dictionary<int, BlockStateModel> finalTable = new Dictionary<int, BlockStateModel>();

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

            // Load and apply block render types...
            string renderTypePath = GetPacksDirectory() + "/block_render_type.json";
            if (File.Exists(renderTypePath))
            {
                try
                {
                    string renderTypeText = File.ReadAllText(renderTypePath);
                    var renderTypes = Json.ParseJson(renderTypeText);

                    var lookup = Block.Palette.StateListTable;

                    foreach (var typeItem in renderTypes.Properties)
                    {
                        var blockId = ResourceLocation.fromString(typeItem.Key);

                        if (lookup.ContainsKey(blockId))
                        {
                            foreach (var stateId in lookup[blockId])
                            {
                                if (finalTable.ContainsKey(stateId))
                                {
                                    finalTable[stateId].SetRenderType(
                                        typeItem.Value.StringValue.ToLower() switch
                                        {
                                            "solid"         => RenderType.SOLID,
                                            "cutout"        => RenderType.CUTOUT,
                                            "cutout_mipped" => RenderType.CUTOUT_MIPPED,
                                            "translucent"   => RenderType.TRANSLUCENT,

                                            _               => RenderType.SOLID
                                        }
                                    );
                                }

                            }
                        }

                    }

                }
                catch (Exception e)
                {
                    Debug.LogWarning("Failed to load block render types: " + e.Message);
                }
            }
            else
            {
                Debug.LogWarning("Block render type definitions not found at " + renderTypePath);
            }

            Debug.Log("Resource packs loaded in " + (Time.realtimeSinceStartup - startTime) + " seconds.");
            Debug.Log("Built " + finalTable.Count + " block state geometry lists.");

        }


    }
}