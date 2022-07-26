using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

using MinecraftClient.Resource;

namespace MinecraftClient.BlockData
{
    public class BlockManager
    {
        public static string GetRootDirectory()
        {
            return Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/');
        }

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

    }

}
