using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MinecraftClient.Mapping.BlockStatePalettes
{
    public abstract class BlockStatePalette
    {
        public BlockStatePalette()
        {
            ReadBlockStates();
        }

        public BlockState FromId(int stateId)
        {
            return statesTable.GetValueOrDefault(stateId, BlockState.AIR_STATE);
        }

        public ResourceLocation GetBlock(int stateId)
        {
            return blocksTable.GetValueOrDefault(stateId, BlockState.AIR_ID);
        }

        public HashSet<int> GetStatesOfBlock(ResourceLocation blockId)
        {
            return stateListTable.GetValueOrDefault(blockId, new HashSet<int>());
        }



        private bool blockStatesReady = false, buzy = false;
        public bool BlockStatesReady
        {
            get {
                return blockStatesReady;
            }
        }

        public static string GetRootDirectory()
        {
            return Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/');
        }

        private Dictionary<ResourceLocation, HashSet<int>> stateListTable = new Dictionary<ResourceLocation, HashSet<int>>();
        public Dictionary<ResourceLocation, HashSet<int>> StateListTable { get { return stateListTable; } }

        private Dictionary<int, ResourceLocation> blocksTable = new Dictionary<int, ResourceLocation>();
        public Dictionary<int, ResourceLocation> BlocksTable { get { return blocksTable; } }

        private Dictionary<int, BlockState> statesTable = new Dictionary<int, BlockState>();
        public Dictionary<int, BlockState> StatesTable { get { return statesTable; } }
        
        private List<int> waterLoggedStates = new List<int>();

        protected abstract string GetBlockStatesFile();

        public void ReadBlockStates()
        {
            if (buzy || blockStatesReady) return;

            buzy = true;

            // Clean up first...
            statesTable.Clear();
            blocksTable.Clear();
            stateListTable.Clear();
            waterLoggedStates.Clear();

            HashSet<int> knownStates = new HashSet<int>();
            string fullPath = GetRootDirectory() + "/Server Data/" + GetBlockStatesFile();

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("Block state data not found!");
            }

            Json.JSONData palette = Json.ParseJson(File.ReadAllText(fullPath, Encoding.UTF8)); // TODO
            Debug.Log("Reading block states from " + fullPath);

            foreach (KeyValuePair<string, Json.JSONData> item in palette.Properties)
            {
                ResourceLocation blockId = ResourceLocation.fromString(item.Key);

                if (stateListTable.ContainsKey(blockId))
                    throw new InvalidDataException("Duplicate block id " + blockId.ToString() + "!");
                
                stateListTable[blockId] = new HashSet<int>();

                foreach (Json.JSONData state in item.Value.Properties["states"].DataArray)
                {
                    int stateId = int.Parse(state.Properties["id"].StringValue);

                    if (knownStates.Contains(stateId))
                        throw new InvalidDataException("Duplicate state id " + stateId + "!?");

                    knownStates.Add(stateId);
                    blocksTable[stateId] = blockId;
                    stateListTable[blockId].Add(stateId);

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
                                waterLoggedStates.Add(stateId);
                            }
                        }

                        statesTable[stateId] = new BlockState(blockId, props);
                    }
                    else
                    {
                        statesTable[stateId] = new BlockState(blockId);
                    }

                }
            }

            Debug.Log(statesTable.Count.ToString() + " block states loaded.");

            blockStatesReady = true;
            buzy = false;

        }

    }
}
