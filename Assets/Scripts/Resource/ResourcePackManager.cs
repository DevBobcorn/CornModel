using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using MinecraftClient.BlockData;

namespace MinecraftClient.Resource
{
    public class ResourcePackManager
    {
        private static bool buzy = false;
        private static bool blockStatesReady = false;
        public static bool BlockStatesReady
        {
            get {
                return blockStatesReady;
            }
        }

        private static Dictionary<ResourceLocation, HashSet<int>> blocksTable = new Dictionary<ResourceLocation, HashSet<int>>();
        public static Dictionary<ResourceLocation, HashSet<int>> BlocksTable
        {
            get {
                return blocksTable;
            }
        }

        private static Dictionary<int, BlockState> statesTable = new Dictionary<int, BlockState>();
        public static Dictionary<int, BlockState> StatesTable
        {
            get {
                return statesTable;
            }
        }
        
        private static List<int> waterLoggedStates = new List<int>();

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

        // Make it static because it is shared by all resource packs and managers
        public static void ReadServerBlocks()
        {
            if (buzy) return;

            buzy = true;
            blockStatesReady = false;
            // Clean up first...
            statesTable.Clear();
            waterLoggedStates.Clear();

            HashSet<int> knownStates = new HashSet<int>();

            Json.JSONData palette = Json.ParseJson(File.ReadAllText(GetRootDirectory() + "/Server Data/blocks.json", Encoding.UTF8)); // TODO
            foreach (KeyValuePair<string, Json.JSONData> item in palette.Properties)
            {
                ResourceLocation blockId = ResourceLocation.fromString(item.Key);

                if (blocksTable.ContainsKey(blockId))
                    throw new InvalidDataException("Duplicate block type " + blockId.ToString() + "!");
                
                blocksTable[blockId] = new HashSet<int>();

                foreach (Json.JSONData state in item.Value.Properties["states"].DataArray)
                {
                    int id = int.Parse(state.Properties["id"].StringValue);

                    if (knownStates.Contains(id))
                        throw new InvalidDataException("Duplicate state id " + id + "!?");

                    knownStates.Add(id);
                    blocksTable[blockId].Add(id);

                    if (state.Properties.ContainsKey("properties"))
                    {
                        // This block state contains block properties
                        var props = new Dictionary<string, string>();
                        foreach (var prop in state.Properties["properties"].Properties)
                        {
                            props.Add(prop.Key, prop.Value.StringValue);
                            // Store interesting properties for later use...
                            if (prop.Key == "waterlogged" && prop.Value.StringValue == "true")
                            {
                                waterLoggedStates.Add(id);
                            }
                        }

                        statesTable[id] = new BlockState(blockId, props);
                    }
                    else
                    {
                        statesTable[id] = new BlockState(blockId);
                    }

                }
            }

            blockStatesReady = true;
            buzy = false;

        }

        // Identifier -> Texture path
        public readonly Dictionary<ResourceLocation, string> textureTable = new Dictionary<ResourceLocation, string>();

        // Identifier -> Block model
        public readonly Dictionary<ResourceLocation, BlockModel> modelsTable = new Dictionary<ResourceLocation, BlockModel>();

        // Block state numeral id -> Block state geometries (One single block state may have a list models to use randomly)
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