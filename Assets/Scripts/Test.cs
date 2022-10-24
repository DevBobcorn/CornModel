using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using TMPro;

using MinecraftClient;
using MinecraftClient.Rendering;
using MinecraftClient.Resource;
using MinecraftClient.Mapping;

public class Test : MonoBehaviour
{
    private static readonly ResourceLocation WATER_STILL = new("block/water_still");
    private static readonly ResourceLocation LAVA_STILL  = new("block/lava_still");
    
    private static readonly Color32 TINT_COLOR = new(255, 255, 255, 255);

    private readonly LoadStateInfo loadStateInfo = new();

    public TMP_Text infoText;

    public void TestBuildState(string name, int stateId, BlockState state, BlockStateModel stateModel, int cullFlags, World world, float3 pos)
    {
        int altitude = 0;
        foreach (var model in stateModel.Geometries)
        {
            var coord = pos + new int3(0, altitude, 0);
            var loc = new Location(coord.z, coord.y, coord.x);

            var modelObject = new GameObject(name);
            modelObject.transform.parent = transform;
            modelObject.transform.localPosition = coord;

            var filter = modelObject.AddComponent<MeshFilter>();
            var render = modelObject.AddComponent<MeshRenderer>();

            var collider = modelObject.AddComponent<MeshCollider>();

            // Make and set mesh...
            var visualBuffer = new VertexBuffer();

            if (state.InWater)
                FluidGeometry.Build(ref visualBuffer, WATER_STILL, 0, 0, 0, cullFlags, world.GetWaterColor(loc));
            else if (state.InLava)
                FluidGeometry.Build(ref visualBuffer, LAVA_STILL,  0, 0, 0, cullFlags, BlockGeometry.DEFAULT_COLOR);

            int fluidVertexCount = visualBuffer.vert.Length;
            int fluidTriIdxCount = (fluidVertexCount / 2) * 3;

            var color = BlockStatePalette.INSTANCE.GetBlockColor(stateId, world, loc, state);
            model.Build(ref visualBuffer, float3.zero, cullFlags, color);

            int vertexCount = visualBuffer.vert.Length;
            int triIdxCount = (vertexCount / 2) * 3;

            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];

            var vertAttrs = new NativeArray<VertexAttributeDescriptor>(3, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
            vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 2, stream: 1);
            vertAttrs[2]  = new(VertexAttribute.Color,    dimension: 3, stream: 2);

            // Set mesh params
            meshData.SetVertexBufferParams(vertexCount, vertAttrs);
            vertAttrs.Dispose();

            meshData.SetIndexBufferParams(triIdxCount, IndexFormat.UInt32);

            // Set vertex data
            // Positions
            var positions = meshData.GetVertexData<float3>(0);
            positions.CopyFrom(visualBuffer.vert);
            // Tex Coordinates
            var texCoords = meshData.GetVertexData<float2>(1);
            texCoords.CopyFrom(visualBuffer.txuv);
            // Vertex colors
            var vertColors = meshData.GetVertexData<float3>(2);
            vertColors.CopyFrom(visualBuffer.tint);

            // Set face data
            var triIndices = meshData.GetIndexData<uint>();
            uint vi = 0; int ti = 0;
            for (;vi < vertexCount;vi += 4U, ti += 6)
            {
                triIndices[ti]     = vi;
                triIndices[ti + 1] = vi + 3U;
                triIndices[ti + 2] = vi + 2U;
                triIndices[ti + 3] = vi;
                triIndices[ti + 4] = vi + 1U;
                triIndices[ti + 5] = vi + 3U;
            }

            var bounds = new Bounds(new Vector3(0.5F, 0.5F, 0.5F), new Vector3(1F, 1F, 1F));

            if (state.InWater || state.InLava)
            {
                meshData.subMeshCount = 2;
                meshData.SetSubMesh(0, new SubMeshDescriptor(0, fluidTriIdxCount)
                {
                    bounds = bounds,
                    vertexCount = vertexCount
                }, MeshUpdateFlags.DontRecalculateBounds);
                meshData.SetSubMesh(1, new SubMeshDescriptor(fluidTriIdxCount, triIdxCount - fluidTriIdxCount)
                {
                    bounds = bounds,
                    vertexCount = vertexCount
                }, MeshUpdateFlags.DontRecalculateBounds);
            }
            else
            {
                meshData.subMeshCount = 1;
                meshData.SetSubMesh(0, new SubMeshDescriptor(0, triIdxCount)
                {
                    bounds = bounds,
                    vertexCount = vertexCount
                }, MeshUpdateFlags.DontRecalculateBounds);
            }

            var mesh = new Mesh
            {
                bounds = bounds,
                name = "Proc Mesh"
            };

            Mesh.ApplyAndDisposeWritableMeshData(meshDataArr, mesh);

            // Recalculate mesh normals
            mesh.RecalculateNormals();

            filter.sharedMesh   = mesh;
            collider.sharedMesh = mesh;

            if (state.InWater)
            {
                render.sharedMaterials =
                    new []{
                        MaterialManager.GetBlockMaterial(RenderType.TRANSLUCENT),
                        MaterialManager.GetBlockMaterial(BlockStatePalette.INSTANCE.GetRenderType(stateId))
                    };
            }
            else
                render.sharedMaterial = MaterialManager.GetBlockMaterial(BlockStatePalette.INSTANCE.GetRenderType(stateId));

            altitude -= 2;

        }

    }

    private IEnumerator DoBuild(string dataVersion, string resourceVersion)
    {
        var wait = new WaitForSecondsRealtime(0.1F);

        // First load all possible Block States...
        var blockLoadFlag = new CoroutineFlag();

        StartCoroutine(BlockStatePalette.INSTANCE.PrepareData(dataVersion, blockLoadFlag, loadStateInfo));

        while (!blockLoadFlag.done)
            yield return wait;

        // Load texture atlas... TODO (Will be decently implemented in future)
        var atlasLoadFlag = new CoroutineFlag();
        StartCoroutine(AtlasManager.Load(resourceVersion, atlasLoadFlag, loadStateInfo));

        while (!atlasLoadFlag.done)
            yield return wait;

        // Create a new resource pack manager...
        var packManager = new ResourcePackManager();

        // Load resources...
        packManager.ClearPacks();

        ResourcePack pack = new ResourcePack($"vanilla-{resourceVersion}");
        packManager.AddPack(pack);

        // Load valid packs...
        var resLoadFlag = new CoroutineFlag();
        StartCoroutine(packManager.LoadPacks(resLoadFlag, loadStateInfo));

        while (!resLoadFlag.done)
            yield return wait;

        float startTime = Time.realtimeSinceStartup;

        int start = 64, limit = 4096;
        int count = 0, width = 64;

        // Create a placeholder world as provider of block colors
        var world = new World();

        foreach (var item in packManager.finalTable)
        {
            int index = count - start;
            if (index >= 0)
            {
                var state = BlockStatePalette.INSTANCE.StatesTable[item.Key];

                TestBuildState($"{item.Key} {state}", item.Key, state, item.Value, 0b111111, world, new((index % width) * 2, 0, (index / width) * 2));
            }

            count++;

            if (count >= start + limit)
                break;

        }

        loadStateInfo.infoText = $"Minecraft block meshes built in {Time.realtimeSinceStartup - startTime} seconds.";
    }

    void Start()
    {
        StartCoroutine(DoBuild("1.16", "1.16.5"));

    }

    void Update()
    {
        if (infoText is not null)
        {
            if (infoText.text != loadStateInfo.infoText)
                infoText.text = loadStateInfo.infoText;

        }

    }

}
