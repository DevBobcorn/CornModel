using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using MinecraftClient.Rendering;

namespace MinecraftClient.Resource
{
    public class ItemGeometry
    {
        public const float MC_VERT_SCALE = 16F;
        public const float MC_UV_SCALE = 16F;

        public readonly List<float3> verticies = new();
        public readonly List<float2> uvs       = new();
        public readonly List<int> tintIndices  = new();

        public void AppendWrapper(BlockModelWrapper wrapper)
        {
            // Build things up!
            foreach (var elem in wrapper.model.elements)
            {
                AppendElement(wrapper.model, elem, wrapper.zyRot, wrapper.uvlock);
            }
        }

        private float3[] vertexArr = { };
        private float2[] txuvArr = { };
        private int[]    tintArr = { };

        public ItemGeometry Finalize()
        {
            vertexArr = verticies.ToArray();
            txuvArr = uvs.ToArray();
            tintArr = tintIndices.ToArray();

            return this;
        }

        public void Build(ref VertexBuffer buffer, float3 posOffset, float3 itemTint)
        {
            int vertexCount = buffer.vert.Length + vertexArr.Length;

            var verts = new float3[vertexCount];
            var txuvs = new float2[vertexCount];
            var tints = new float3[vertexCount];

            buffer.vert.CopyTo(verts, 0);
            buffer.txuv.CopyTo(txuvs, 0);
            buffer.tint.CopyTo(tints, 0);

            uint i, vertOffset = (uint)buffer.vert.Length;

            if (vertexArr.Length > 0)
            {
                for (i = 0U;i < vertexArr.Length;i++)
                {
                    verts[i + vertOffset] = vertexArr[i] + posOffset;
                    tints[i + vertOffset] = tintArr[i] >= 0 ? itemTint : BlockGeometry.DEFAULT_COLOR;
                }
                txuvArr.CopyTo(txuvs, vertOffset);
                vertOffset += (uint)vertexArr.Length;
            }

            buffer.vert = verts;
            buffer.txuv = txuvs;
            buffer.tint = tints;

        }

        private void AppendElement(JsonModel model, JsonModelElement elem, int2 zyRot, bool uvlock)
        {
            float lx = Mathf.Min(elem.from.x, elem.to.x) / MC_VERT_SCALE;
            float mx = Mathf.Max(elem.from.x, elem.to.x) / MC_VERT_SCALE;
            float ly = Mathf.Min(elem.from.y, elem.to.y) / MC_VERT_SCALE;
            float my = Mathf.Max(elem.from.y, elem.to.y) / MC_VERT_SCALE;
            float lz = Mathf.Min(elem.from.z, elem.to.z) / MC_VERT_SCALE;
            float mz = Mathf.Max(elem.from.z, elem.to.z) / MC_VERT_SCALE;

            float3[] elemVerts = new float3[]{
                new float3(lx, ly, lz), new float3(lx, ly, mz),
                new float3(lx, my, lz), new float3(lx, my, mz),
                new float3(mx, ly, lz), new float3(mx, ly, mz),
                new float3(mx, my, lz), new float3(mx, my, mz)
            };

            if (elem.rotAngle != 0F) // Apply model rotation...
                Rotations.RotateVertices(ref elemVerts, elem.pivot / MC_VERT_SCALE, elem.axis, -elem.rotAngle, elem.rescale); // TODO Check angle
            
            bool stateRotated = zyRot.x != 0 || zyRot.y != 0;

            if (stateRotated) // Apply state rotation...
                Rotations.RotateWrapper(ref elemVerts, zyRot);

            foreach (var facePair in elem.faces)
            {
                // Select the current face
                var face = facePair.Value;

                switch (facePair.Key) // Build face in that direction
                {
                    case FaceDir.UP:    // Unity +Y
                        verticies.Add(elemVerts[2]); // 0
                        verticies.Add(elemVerts[3]); // 1
                        verticies.Add(elemVerts[6]); // 2
                        verticies.Add(elemVerts[7]); // 3
                        break;
                    case FaceDir.DOWN:  // Unity -Y
                        verticies.Add(elemVerts[4]); // 0
                        verticies.Add(elemVerts[5]); // 1
                        verticies.Add(elemVerts[0]); // 2
                        verticies.Add(elemVerts[1]); // 3
                        break;
                    case FaceDir.SOUTH: // Unity +X
                        verticies.Add(elemVerts[6]); // 0
                        verticies.Add(elemVerts[7]); // 1
                        verticies.Add(elemVerts[4]); // 2
                        verticies.Add(elemVerts[5]); // 3
                        break;
                    case FaceDir.NORTH: // Unity -X
                        verticies.Add(elemVerts[3]); // 0
                        verticies.Add(elemVerts[2]); // 1
                        verticies.Add(elemVerts[1]); // 2
                        verticies.Add(elemVerts[0]); // 3
                        break;
                    case FaceDir.EAST:  // Unity +Z
                        verticies.Add(elemVerts[7]); // 0
                        verticies.Add(elemVerts[3]); // 1
                        verticies.Add(elemVerts[5]); // 2
                        verticies.Add(elemVerts[1]); // 3
                        break;
                    case FaceDir.WEST:  // Unity -Z
                        verticies.Add(elemVerts[2]); // 0
                        verticies.Add(elemVerts[6]); // 1
                        verticies.Add(elemVerts[0]); // 2
                        verticies.Add(elemVerts[4]); // 3
                        break;
                }

                ResourceLocation texIdentifier = model.resolveTextureName(face.texName);

                // This value is mapped only when uvlock is on, according to this block state's
                // state rotation, and it rotates the area of texture which is used on the face
                int uvAreaRot = stateRotated && uvlock ? uvlockMap[zyRot][facePair.Key] : 0;

                float2[] remappedUVs = RemapUVs(face.uv / MC_UV_SCALE, texIdentifier, uvAreaRot);

                // This rotation doesn't change the area of texture used...
                // See https://minecraft.fandom.com/wiki/Model#Block_models
                switch (face.rot)
                {
                    case Rotations.UVRot.UV_90:
                        uvs.Add(remappedUVs[2]); // 2
                        uvs.Add(remappedUVs[0]); // 0
                        uvs.Add(remappedUVs[3]); // 3
                        uvs.Add(remappedUVs[1]); // 1
                        break;
                    case Rotations.UVRot.UV_180:
                        uvs.Add(remappedUVs[3]); // 3
                        uvs.Add(remappedUVs[2]); // 2
                        uvs.Add(remappedUVs[1]); // 1
                        uvs.Add(remappedUVs[0]); // 0
                        break;
                    case Rotations.UVRot.UV_270:
                        uvs.Add(remappedUVs[1]); // 1
                        uvs.Add(remappedUVs[3]); // 3
                        uvs.Add(remappedUVs[0]); // 0
                        uvs.Add(remappedUVs[2]); // 2
                        break;
                    default: // Including Rotations.UVRot.UV_0
                        uvs.Add(remappedUVs[0]); // 0
                        uvs.Add(remappedUVs[1]); // 1
                        uvs.Add(remappedUVs[2]); // 2
                        uvs.Add(remappedUVs[3]); // 3
                        break;
                }
                
                // And tint indices..
                for (int i = 0;i < 4;i++)
                    tintIndices.Add(face.tintIndex);
                
            }

        }

        private static float2[] RemapUVs(float4 uvs, ResourceLocation source, int areaRot)
        {
            return AtlasManager.GetUVs(source, uvs, areaRot);
        }

        private static Dictionary<int2, Dictionary<FaceDir, int>> CreateUVLockMap()
        {
            var areaRotMap = new Dictionary<int2, Dictionary<FaceDir, int>>();

            for (int roty = 0;roty < 4;roty++)
            {
                for (int rotz = 0;rotz < 4;rotz++)
                {
                    // Store actual rotation values currently applied to these faces (due to vertex(mesh) rotation)
                    var localRot = new Dictionary<FaceDir, int>();

                    foreach (FaceDir dir in Enum.GetValues(typeof (FaceDir)))
                        localRot.Add(dir, 0);

                    switch (rotz)
                    {
                        case 0:
                            localRot[FaceDir.UP]   =  roty;
                            localRot[FaceDir.DOWN] = -roty;
                            break;
                        case 1: // Locally rotate 90 Deg Clockwise
                            localRot[FaceDir.UP]    =  2;
                            localRot[FaceDir.DOWN]  =  0;
                            localRot[FaceDir.WEST]  = -1;
                            localRot[FaceDir.EAST]  =  1;
                            localRot[FaceDir.SOUTH] =  roty;
                            localRot[FaceDir.NORTH] = -roty + 2;
                            break;
                        case 2: // Locally rotate 180 Deg
                            localRot[FaceDir.UP]    = -roty;
                            localRot[FaceDir.DOWN]  =  roty;
                            localRot[FaceDir.WEST]  =  2;
                            localRot[FaceDir.EAST]  =  2;
                            localRot[FaceDir.SOUTH] =  2;
                            localRot[FaceDir.NORTH] =  2;
                            break;
                        case 3: // Locally rotate 90 Deg Counter-Clockwise
                            localRot[FaceDir.UP]    =  0;
                            localRot[FaceDir.DOWN]  =  2;
                            localRot[FaceDir.WEST]  =  1;
                            localRot[FaceDir.EAST]  = -1;
                            localRot[FaceDir.SOUTH] = -roty;
                            localRot[FaceDir.NORTH] =  roty + 2;
                            break;
                    }

                    var result = new Dictionary<FaceDir, int>();

                    // Cancel horizontal texture rotations (front / right / back / left)
                    foreach (FaceDir dir in Enum.GetValues(typeof (FaceDir)))
                        result.Add(dir, (8 - localRot.GetValueOrDefault(dir, 0)) % 4);

                    areaRotMap.Add(new int2(rotz, roty), result);

                }
                
            }
            
            return areaRotMap;

        }

        private static readonly Dictionary<int2, Dictionary<FaceDir, int>> uvlockMap = CreateUVLockMap();
    }
}