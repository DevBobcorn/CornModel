using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;

namespace MinecraftClient.Mapping.BlockPalettes
{
    /// <summary>
    /// Generator for MCC Block Palette mappings
    /// </summary>
    /// <remarks>
    /// Example for generating MaterialXXX.cs and PaletteXXX.cs from blocks.json:
    ///
    /// MinecraftClient.Mapping.BlockPalettes.BlockPaletteGenerator.JsonToClass(
    ///     @"C:\Path\To\blocks.json",
    ///     "116Plus",
    ///     @"C:\Path\To\Output\",
    /// );
    ///
    /// Place the above example inside the Main() method of Program.cs, adjust paths then compile and run.
    /// Do not forget to remove the temporay call to JsonToClass() from Main() once you are done.
    /// </remarks>
    public static class BlockPaletteGenerator
    {
        /// <summary>
        /// Generate mapping from Minecraft blocks.json
        /// </summary>
        /// <param name="blocksJsonFile">path to blocks.json</param>
        /// <param name="nameSuffix">name suffix of the classes to be generated</param>
        /// <param name="outputPath">output path for BlocksXXX.cs and PaletteXXX.cs</param>
        /// <remarks>java -cp minecraft_server.jar net.minecraft.data.Main --reports</remarks>
        /// <returns>state => block name mappings</returns>
        public static void JsonToClass(string blocksJsonFile, string nameSuffix, string outputPath)
        {
            HashSet<int> knownStates = new HashSet<int>();
            Dictionary<string, HashSet<int>> blocks = new Dictionary<string, HashSet<int>>();

            Dictionary<int, Dictionary<string, Json.JSONData>> blocPropsTable = new Dictionary<int, Dictionary<string, Json.JSONData>>();

            Json.JSONData palette = Json.ParseJson(File.ReadAllText(blocksJsonFile, Encoding.UTF8));
            foreach (KeyValuePair<string, Json.JSONData> item in palette.Properties)
            {
                //minecraft:item_name => ItemName
                string blockType = String.Concat(
                    item.Key.Replace("minecraft:", "")
                    .Split('_')
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1))
                );

                if (blocks.ContainsKey(blockType))
                    throw new InvalidDataException("Duplicate block type " + blockType + "!?");
                blocks[blockType] = new HashSet<int>();

                foreach (Json.JSONData state in item.Value.Properties["states"].DataArray)
                {
                    int id = int.Parse(state.Properties["id"].StringValue);

                    if (knownStates.Contains(id))
                        throw new InvalidDataException("Duplicate state id " + id + "!?");

                    knownStates.Add(id);
                    blocks[blockType].Add(id);
                    if (state.Properties.ContainsKey("properties"))
                    {
                        // This block state contains block properties
                        blocPropsTable[id] = state.Properties["properties"].Properties;
                    }
                }
            }

            HashSet<string> materials = new HashSet<string>();
            List<string> outFile = new List<string>();
            outFile.AddRange(new[] {
                "using System;",
                "using System.Collections.Generic;",
                "",
                "namespace MinecraftClient.Mapping.BlockPalettes",
                "{",
                "    public class Palette" + nameSuffix + " : BlockPalette",
                "    {",
                "        private static Dictionary<int, Material> materials = new Dictionary<int, Material>();",
                "        private static Dictionary<int, Dictionary<string, string>> blocProps = new Dictionary<int, Dictionary<string, string>>();",
                "",
                "        static Palette" + nameSuffix +"()",
                "        {",
            });

            foreach (KeyValuePair<string, HashSet<int>> blockType in blocks)
            {
                if (blockType.Value.Count > 0)
                {
                    List<int> idList = blockType.Value.ToList();
                    string materialName = blockType.Key;
                    materials.Add(materialName);

                    if (idList.Count > 1)
                    {
                        idList.Sort();
                        Queue<int> idQueue = new Queue<int>(idList);

                        while (idQueue.Count > 0)
                        {
                            int startValue = idQueue.Dequeue();
                            int endValue = startValue;
                            while (idQueue.Count > 0 && idQueue.Peek() == endValue + 1)
                                endValue = idQueue.Dequeue();
                            if (endValue > startValue)
                            {
                                outFile.Add("            for (int i = " + startValue + "; i <= " + endValue + "; i++)\n            {");
                                outFile.Add("                materials[i] = Material." + materialName + ";");
                                outFile.Add("            }");
                                // For each state, write down their properties
                                for (int i = startValue;i <= endValue;i++)
                                {
                                    //string propsText = string.Empty;
                                    string propsCode = "            blocProps[" + i + "] = new Dictionary<string,string>();";
                                    foreach (var prop in blocPropsTable[i])
                                    {
                                        //propsText += (prop.Key + ":" + prop.Value.StringValue + "\t");
                                        propsCode += "blocProps[" + i + "].Add(\"" + prop.Key + "\",\"" + prop.Value.StringValue + "\");";
                                    }
                                    //outFile.Add("            // " + i + "\tProperties: " + propsText);
                                    outFile.Add(propsCode);
                                }
                            }
                            else {
                                outFile.Add("            materials[" + startValue + "] = Material." + materialName + ";");
                                
                            }
                        }
                    }
                    else
                    {
                        // This block has only 1 blockstate. In this case it's not really possible for it to have
                        // block properties. Just skip. :)
                        outFile.Add("            materials[" + idList[0] + "] = Material." + materialName + ";");
                    }
                }
                else throw new InvalidDataException("No state id  for block type " + blockType.Key + "!?");
            }

            outFile.AddRange(new[] {
                "        }",
                "",
                "        protected override Dictionary<int, Material> GetDict()",
                "        {",
                "            return materials;",
                "        }",
                "    }",
                "}"
            });

            File.WriteAllLines(outputPath + @"\Palette" + nameSuffix + ".cs", outFile);

            if (outputPath != null)
            {
                outFile = new List<string>();
                outFile.AddRange(new[] {
                    "namespace MinecraftClient.Mapping",
                    "{",
                    "    public enum Material",
                    "    {"
                });
                foreach (string material in materials)
                    outFile.Add("        " + material + ",");
                outFile.AddRange(new[] {
                    "    }",
                    "}"
                });
                File.WriteAllLines(outputPath + @"\Material" + nameSuffix + ".cs", outFile);
            }
        }
    }
}
