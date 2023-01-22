using System.IO;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using MinecraftClient.Rendering;
using MinecraftClient.Resource;

namespace MinecraftClient.Magic
{
    public static class ParticleVoxelGenerator
    {
        private const float SPACE_STEP = 0.125F; // 1 / 8
        //private const float SPACE_STEP = 0.0625F; // 1 / 16

        private const float SCALE = 0.1F;

        private static readonly float3 SCALE_3 = new(SCALE, SCALE, SCALE);

        private static void MakeVoxel(Transform root, float3 pos, Color32 color, int faceIndex)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube);

            obj.transform.SetParent(root);

            obj.transform.localPosition =  pos;
            obj.transform.localScale = SCALE_3;

            obj.GetComponent<MeshRenderer>().material.color = color;

            obj.name = $"Voxel on face #{faceIndex}";
        }

        public static void Generate(Transform root, Color32[][] atlasData, VertexBuffer buffer)
        {
            var funcPath = $"D:/Minecraft/Generated/voxel_particle.mcfunction";

            int vertCnt = buffer.vert.Length;

            var lines = new List<string>();

            // Magic starts...
            for (int vi = 0;vi < vertCnt;vi += 4) // For each quad in the geometry
            {
                var quadOrg = buffer.vert[vi];
                var faceIdx = vi / 4;

                var veci = buffer.vert[vi + 1] - quadOrg;
                var vecj = buffer.vert[vi + 2] - quadOrg;

                var uvOrg = buffer.txuv[vi].xy;
                var vecu = buffer.txuv[vi + 1].xy - uvOrg;
                var vecv = buffer.txuv[vi + 2].xy - uvOrg;

                float3 blockTint = buffer.tint[vi];

                for (float i = 0F;i < math.length(veci);i += SPACE_STEP)
                    for (float j = 0F;j < math.length(vecj);j += SPACE_STEP)
                    {
                        float pi = i / math.length(veci);
                        float pj = j / math.length(vecj);

                        // Remap u and v from [0.00, 1.00] to [0.05, 0.95]
                        float pu = pi * 0.9F + 0.05F;
                        float pv = pj * 0.9F + 0.05F;

                        var uvCoord = uvOrg + vecu * pu + vecv * pv;

                        int atlasIndex = (int)buffer.txuv[vi].z;
                        int sampleX = Mathf.RoundToInt(uvCoord.x * AtlasManager.ATLAS_SIZE);
                        int sampleY = Mathf.RoundToInt(uvCoord.y * AtlasManager.ATLAS_SIZE);

                        //Debug.Log($"Sampling at [{atlasIndex}] {uvCoord.x} {uvCoord.y}");

                        Color voxelColor = atlasData[atlasIndex][AtlasManager.ATLAS_SIZE * sampleY + sampleX];

                        if (voxelColor.a < 0.5F)
                            continue; // Discard
                        
                        float r = voxelColor.r * blockTint.x;
                        float g = voxelColor.g * blockTint.y;
                        float b = voxelColor.b * blockTint.z;
                        float a =               voxelColor.a;

                        voxelColor = new Color32(
                            (byte)(r * 255F),
                            (byte)(g * 255F),
                            (byte)(b * 255F),
                            (byte)(a * 255F)
                        );

                        var pos = quadOrg + veci * pi + vecj * pj;

                        // Visualize voxel in scene
                        MakeVoxel(root, pos, voxelColor, faceIdx);

                        // Write voxel into text (Swap x and z)
                        lines.Add($"particle minecraft:dust {r:0.000} {g:0.000} {b:0.000} 1 ~{pos.z:0.000} ~{pos.y:0.000} ~{pos.x:0.000} 0 0 0 0 1 force @a");

                    }

            }

            File.WriteAllLines(funcPath, lines);
        }
    }
}