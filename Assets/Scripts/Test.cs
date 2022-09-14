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
using MinecraftClient.Mapping.BlockStatePalettes;

public class Test : MonoBehaviour
{
    private static readonly Color32 TINTCOLOR = new Color32(180, 255, 255, 255);

    private readonly LoadStateInfo loadStateInfo = new();

    public TMP_Text infoText;

    public void TestBuildState(string name, int stateId, BlockStateModel stateModel, int cullFlags, bool buildWater, float3 pos)
    {
        int altitude = 0;
        foreach (var model in stateModel.Geometries)
        {
            var coord = pos + new int3(0, altitude, 0);

            var modelObject = new GameObject(name);
            modelObject.transform.parent = transform;
            modelObject.transform.localPosition = coord;

            var filter = modelObject.AddComponent<MeshFilter>();
            var render = modelObject.AddComponent<MeshRenderer>();

            // Make and set mesh...
            var visualBuffer = new VertexBuffer();

            if (buildWater)
                FluidGeometry.Build(ref visualBuffer, 0, 0, 0, cullFlags);
            int fluidVertexCount = visualBuffer.vert.Length;
            int fluidTriIdxCount = (fluidVertexCount / 2) * 3;
            
            model.Build(ref visualBuffer, float3.zero, cullFlags);

            int vertexCount = visualBuffer.vert.Length;
            int triIdxCount = (vertexCount / 2) * 3;

            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];

            var vertAttrs = new NativeArray<VertexAttributeDescriptor>(2, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
            vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 2, stream: 1);

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

            if (buildWater)
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

            filter.sharedMesh = mesh;
            if (buildWater)
            {
                render.sharedMaterials =
                    new []{
                        MaterialManager.GetBlockMaterial(RenderType.TRANSLUCENT),
                        MaterialManager.GetBlockMaterial(Block.Palette.GetRenderType(stateId))
                    };
            }
            else
                render.sharedMaterial = MaterialManager.GetBlockMaterial(Block.Palette.GetRenderType(stateId));

            altitude -= 2;

        }

    }

    private IEnumerator DoBuild(string dataVersion, string resourceVersion)
    {
        var wait = new WaitForSecondsRealtime(0.1F);

        // First load all possible Block States...
        var blockLoadFlag = new CoroutineFlag();

        Block.Palette = new BlockStatePalette();
        StartCoroutine(Block.Palette.PrepareData(dataVersion, blockLoadFlag, loadStateInfo));

        while (!blockLoadFlag.done)
            yield return wait;

        // Load texture atlas... TODO (Will be decently implemented in future)
        BlockTextureManager.EnsureInitialized();
        var atlasLoadFlag = new CoroutineFlag();
        StartCoroutine(BlockTextureManager.Load(resourceVersion, atlasLoadFlag, loadStateInfo));

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

        int start = 0, limit = 4096;
        int count = 0, width = 64;
        foreach (var item in packManager.finalTable)
        {
            int index = count - start;
            if (index >= 0)
            {
                var state = Block.Palette.StatesTable[item.Key];

                TestBuildState($"{item.Key} {state}", item.Key, item.Value, 0b111111, state.InWater, new((index % width) * 2, 0, (index / width) * 2));
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
