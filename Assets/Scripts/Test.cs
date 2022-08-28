using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;

using MinecraftClient;
using MinecraftClient.Rendering;
using MinecraftClient.Resource;
using MinecraftClient.Mapping;
using MinecraftClient.Mapping.BlockStatePalettes;

public class Test : MonoBehaviour
{
    private static readonly Color32 TINTCOLOR = new Color32(180, 255, 255, 255);

    private Mesh GetMeshFromData(Tuple<float3[], float2[], int[], uint[]> geoData)
    {
        int vertCount = geoData.Item1.Length;
        int triIndexCount = geoData.Item4.Length;

        var meshDataArr = Mesh.AllocateWritableMeshData(1);
        var meshData = meshDataArr[0];

        var vertAttrs = new NativeArray<VertexAttributeDescriptor>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
        vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 2, stream: 1);

        // Set mesh params
        meshData.SetVertexBufferParams(vertCount, vertAttrs);
        vertAttrs.Dispose();

        meshData.SetIndexBufferParams(triIndexCount, IndexFormat.UInt32);

        // Set vertex data
        // Positions
        var positions = meshData.GetVertexData<float3>(0);
        positions.CopyFrom(geoData.Item1);
        // Tex Coordinates
        var texCoords = meshData.GetVertexData<float2>(1);
        texCoords.CopyFrom(geoData.Item2);

        // Set face data
        var triIndices = meshData.GetIndexData<uint>();
        triIndices.CopyFrom(geoData.Item4);

        var bounds = new Bounds(new Vector3(0.5F, 0.5F, 0.5F), new Vector3(1F, 1F, 1F));

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, triIndexCount)
        {
            bounds = bounds,
            vertexCount = vertCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Proc Mesh"
        };

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArr, mesh);

        return mesh;

    }

    public void TestBuildModel(string name, BlockModel model, int cullFlags, float3 pos, RenderType renderType)
    {
        // First prepare our model data
        var wrapper = new BlockModelWrapper(model, int2.zero, false);
        var geometry = new BlockGeometry(wrapper).Finalize();

        var modelObject = new GameObject(name);
        modelObject.transform.parent = transform;
        modelObject.transform.localPosition = pos;

        var filter = modelObject.AddComponent<MeshFilter>();
        var render = modelObject.AddComponent<MeshRenderer>();

        // Make and set mesh...
        Mesh mesh = GetMeshFromData(geometry.GetData(cullFlags));

        filter.sharedMesh = mesh;
        render.sharedMaterial = MaterialManager.GetBlockMaterial(renderType);

    }

    public void TestUVLock(string name, BlockModel model, RenderType type, int cullFlags, float3 pos)
    {
        for (int yrot = 0;yrot < 4;yrot++)
            for (int zrot = 0;zrot < 4;zrot++)
            {
                // First prepare our model data
                var wrapper = new BlockModelWrapper(model, new int2(zrot, yrot), false);
                var geometry = new BlockGeometry(wrapper).Finalize();
                var geoData = geometry.GetData(cullFlags);

                var modelObject = new GameObject(name + " yr=" + yrot + ", zr=" + zrot);
                modelObject.transform.parent = transform;
                modelObject.transform.localPosition = pos + new float3(zrot * 2, 0, yrot * 2);

                var filter = modelObject.AddComponent<MeshFilter>();
                var render = modelObject.AddComponent<MeshRenderer>();

                // Make and set mesh...
                Mesh mesh = GetMeshFromData(geometry.GetData(cullFlags));

                filter.sharedMesh = mesh;
                render.sharedMaterial = MaterialManager.GetBlockMaterial(type);
            }
    }

    public void TestBuildState(string name, int stateId, BlockStateModel stateModel, int cullFlags, float3 pos)
    {
        int altitude = 0;
        foreach (var geometry in stateModel.Geometries)
        {
            // First prepare our model data
            var geoData = geometry.GetData(cullFlags);

            var modelObject = new GameObject(name);
            modelObject.transform.parent = transform;
            modelObject.transform.localPosition = pos + new int3(0, altitude, 0);

            var filter = modelObject.AddComponent<MeshFilter>();
            var render = modelObject.AddComponent<MeshRenderer>();

            // Make and set mesh...
            Mesh mesh = GetMeshFromData(geometry.GetData(cullFlags));

            filter.sharedMesh = mesh;
            render.sharedMaterial = MaterialManager.GetBlockMaterial(Block.Palette.GetRenderType(stateId));

            altitude -= 2;

        }

    }

    void Start()
    {
        // First load all possible Block States...
        Block.Palette = new Palette116();

        // Create a new resource pack...
        ResourcePackManager manager = new ResourcePackManager();

        ResourcePack pack = new ResourcePack("vanilla-1.16.5");
        manager.AddPack(pack);

        // Load valid packs...
        manager.LoadPacks();

        float startTime = Time.realtimeSinceStartup;

        int start = 0, limit = 4096;
        int count = 0, width = 64;
        foreach (var item in manager.finalTable)
        {
            int index = count - start;
            if (index >= 0)
            {
                string stateName = Block.Palette.StatesTable[item.Key].ToString();

                TestBuildState(item.Key.ToString() + " " + stateName, item.Key, item.Value, 0b111111, new float3((index % width) * 2, 0, (index / width) * 2));
            }

            count++;

            if (count >= start + limit)
                break;

        }

        TestUVLock("UVLock Test Unit Lower ", manager.modelsTable[ResourceLocation.fromString("block/dir_block")],            RenderType.SOLID,       0b111111, new float3(0, 3, 0));
        TestUVLock("UVLock Test Unit Upper ", manager.modelsTable[ResourceLocation.fromString("block/orange_stained_glass")], RenderType.TRANSLUCENT, 0b111111, new float3(0, 6, 0));

        Debug.Log("Unity meshes built in " + (Time.realtimeSinceStartup - startTime) + " seconds.");

    }
}
