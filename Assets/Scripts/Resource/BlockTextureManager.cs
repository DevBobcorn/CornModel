using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MinecraftClient.Resource
{
    public class BlockTextureManager
    {
        private static Dictionary<ResourceLocation, int> blockAtlasTable = new Dictionary<ResourceLocation, int>();
        private static bool initialized = false;

        public static Vector4 GetUVs(ResourceLocation identifier, Vector4 part)
        {
            return GetUVsAtOffset(GetAtlasOffset(identifier), part);
        }

        private const int TexturesInALine = 32;
        private const float SingleSize = 1.0F / TexturesInALine; // Size of a single block texture

        private static Vector4 GetUVsAtOffset(int offset, Vector4 part)
        {
            // vect: x,  y,  z,  w
            // vect: x1, y1, x2, y2

            part *= SingleSize;

            float blockU = (offset % TexturesInALine) / (float)TexturesInALine;
            float blockV = (offset / TexturesInALine) / (float)TexturesInALine;
            return new Vector4(blockU + part.x, blockV + (SingleSize - part.y), blockU + part.z, blockV + (SingleSize - part.w));
        }        

        private static int GetAtlasOffset(ResourceLocation identifier)
        {
            if (!initialized) Initialze();

            if (blockAtlasTable.ContainsKey(identifier))
                return blockAtlasTable[identifier];
            
            return 0;
        }

        public static Texture2D atlasTexture = new Texture2D(2, 2); // First assign a place holder...
        public static Material atlasMaterial;

        public static void EnsureInitialized()
        {
            if (!initialized) Initialze();
        }

        private static void Initialze()
        {
            Debug.Log("Read and initialize Block Atlas table...");
            string atlasFilePath = ResourcePackManager.GetPacksDirectory() + "/block_atlas.png";
            string atlasJsonPath = ResourcePackManager.GetPacksDirectory() + "/block_atlas_dict.json";

            if (File.Exists(atlasJsonPath) && File.Exists(atlasFilePath))
            {
                // Set up atlas texture...
                atlasTexture.LoadImage(File.ReadAllBytes(atlasFilePath));
                atlasTexture.filterMode = FilterMode.Point;

                // Set up atlas material...
                atlasMaterial = new Material(Shader.Find("Standard"));
                atlasMaterial.EnableKeyword("_ALPHATEST_ON");
                atlasMaterial.SetTexture("_MainTex", atlasTexture);
                atlasMaterial.SetFloat("_Mode", 1);
                atlasMaterial.renderQueue = 2450;
                atlasMaterial.SetFloat("_Glossiness", 0F);
                atlasMaterial.SetFloat("_Cutoff", 0.75F);

                string jsonText = File.ReadAllText(atlasJsonPath);
                Json.JSONData atlasJson = Json.ParseJson(jsonText);

                foreach (KeyValuePair<string, Json.JSONData> item in atlasJson.Properties)
                {
                    if (blockAtlasTable.ContainsKey(ResourceLocation.fromString(item.Key)))
                    {
                        throw new InvalidDataException("Duplicate block atlas with one name " + item.Key + "!?");
                    }
                    else
                    {
                        blockAtlasTable[ResourceLocation.fromString(item.Key)] = int.Parse(item.Value.StringValue);
                    }
                }

            }
            else
            {
                Debug.LogWarning("Texture atlas not available at " + atlasJsonPath + " and " + atlasFilePath);
            }

            initialized = true;

        }
    }
}