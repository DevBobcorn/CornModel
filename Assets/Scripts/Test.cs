using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using TMPro;

using MinecraftClient;
using MinecraftClient.Rendering;
using MinecraftClient.Resource;
using MinecraftClient.Mapping;
using MinecraftClient.Inventory;

public class Test : MonoBehaviour
{
    private static readonly byte[] FLUID_HEIGHTS = new byte[] { 15, 15, 15, 15, 15, 15, 15, 15, 15 };

    [SerializeField] public TMP_Text InfoText;
    [SerializeField] public MaterialManager MaterialManager;

    // Runs before a scene gets loaded
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeApp() => Loom.Initialize();

    public void TestBuildState(string name, int stateId, BlockState state, BlockStateModel stateModel, int cullFlags, AbstractWorld world, float3 pos)
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
                FluidGeometry.Build(ref visualBuffer, FluidGeometry.LiquidTextures[0], 0, 0, 0, FLUID_HEIGHTS,
                        cullFlags, world.GetWaterColor(loc));
            else if (state.InLava)
                FluidGeometry.Build(ref visualBuffer, FluidGeometry.LiquidTextures[1], 0, 0, 0, FLUID_HEIGHTS,
                        cullFlags, BlockGeometry.DEFAULT_COLOR);

            int fluidVertexCount = visualBuffer.vert.Length;
            int fluidTriIdxCount = (fluidVertexCount / 2) * 3;

            var color = BlockStatePalette.INSTANCE.GetBlockColor(stateId, world, loc, state);
            model.Build(ref visualBuffer, float3.zero, cullFlags, color);

            int vertexCount = visualBuffer.vert.Length;
            int triIdxCount = (vertexCount / 2) * 3;

            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];

            var vertAttrs = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
            vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 3, stream: 1);
            vertAttrs[2] = new(VertexAttribute.TexCoord1, dimension: 4, stream: 2);
            vertAttrs[3] = new(VertexAttribute.Color,     dimension: 3, stream: 3);

            // Set mesh params
            meshData.SetVertexBufferParams(vertexCount, vertAttrs);
            vertAttrs.Dispose();

            meshData.SetIndexBufferParams(triIdxCount, IndexFormat.UInt32);

            // Set vertex data
            // Positions
            var positions = meshData.GetVertexData<float3>(0);
            positions.CopyFrom(visualBuffer.vert);
            // Tex Coordinates
            var texCoords = meshData.GetVertexData<float3>(1);
            texCoords.CopyFrom(visualBuffer.txuv);
            // Animation Info
            var animInfos = meshData.GetVertexData<float4>(2);
            animInfos.CopyFrom(visualBuffer.uvan);
            // Vertex colors
            var vertColors = meshData.GetVertexData<float3>(3);
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
                        MaterialManager.GetAtlasMaterial(RenderType.WATER),
                        MaterialManager.GetAtlasMaterial(stateModel.RenderType)
                    };
            }
            else
                render.sharedMaterial = MaterialManager.GetAtlasMaterial(stateModel.RenderType);

            altitude -= 2;

        }

    }

    public void TestBuildItem(string name, int itemNumId, ItemStack itemStack, ItemModel itemModel, float3 pos)
    {
        Dictionary<ItemModelPredicate, ItemGeometry> buildDict = new();

        // Gather all geometries of this model
        buildDict.Add(ItemModelPredicate.EMPTY, itemModel.Geometry);
        foreach (var pair in itemModel.Overrides)
            buildDict.TryAdd(pair.Key, pair.Value);

        int altitude = 0;
        foreach (var pair in buildDict)
        {
            var coord = pos + new int3(0, altitude, 0);

            var modelObject = new GameObject(pair.Key == ItemModelPredicate.EMPTY ? name : $"{name}{pair.Key}");
            modelObject.transform.parent = transform;
            modelObject.transform.localPosition = coord;

            var filter = modelObject.AddComponent<MeshFilter>();
            var render = modelObject.AddComponent<MeshRenderer>();

            var collider = modelObject.AddComponent<MeshCollider>();

            // Make and set mesh...
            var visualBuffer = new VertexBuffer();

            int fluidVertexCount = visualBuffer.vert.Length;
            int fluidTriIdxCount = (fluidVertexCount / 2) * 3;

            float3[] colors;

            var tintFunc = ItemPalette.INSTANCE.GetTintRule(itemNumId);
            if (tintFunc is null)
                colors = new float3[]{ new(1F, 0F, 0F), new(0F, 0F, 1F), new(0F, 1F, 0F) };
            else
                colors = tintFunc.Invoke(itemStack);

            pair.Value.Build(ref visualBuffer, float3.zero, colors);

            int vertexCount = visualBuffer.vert.Length;
            int triIdxCount = (vertexCount / 2) * 3;

            var meshDataArr = Mesh.AllocateWritableMeshData(1);
            var meshData = meshDataArr[0];

            var vertAttrs = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
            vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 3, stream: 1);
            vertAttrs[2] = new(VertexAttribute.TexCoord1, dimension: 4, stream: 2);
            vertAttrs[3] = new(VertexAttribute.Color,     dimension: 3, stream: 3);

            // Set mesh params
            meshData.SetVertexBufferParams(vertexCount, vertAttrs);
            vertAttrs.Dispose();

            meshData.SetIndexBufferParams(triIdxCount, IndexFormat.UInt32);

            // Set vertex data
            // Positions
            var positions = meshData.GetVertexData<float3>(0);
            positions.CopyFrom(visualBuffer.vert);
            // Tex Coordinates
            var texCoords = meshData.GetVertexData<float3>(1);
            texCoords.CopyFrom(visualBuffer.txuv);
            // Animation Info
            var animInfos = meshData.GetVertexData<float4>(2);
            animInfos.CopyFrom(visualBuffer.uvan);
            // Vertex colors
            var vertColors = meshData.GetVertexData<float3>(3);
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

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, triIdxCount)
            {
                bounds = bounds,
                vertexCount = vertexCount
            }, MeshUpdateFlags.DontRecalculateBounds);

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

            render.sharedMaterial = MaterialManager.GetAtlasMaterial(itemModel.RenderType);

            altitude += 2;

        }
    
    }

    class World : AbstractWorld { }

    private IEnumerator DoBuild(string dataVersion, string resourceVersion, string[] resourceOverrides, int itemPrecision)
    {
        // First load all possible Block States...
        var loadFlag = new DataLoadFlag();
        Task.Run(() => BlockStatePalette.INSTANCE.PrepareData(dataVersion, loadFlag));
        while (!loadFlag.Finished) yield return null;

        // Then load all Items...
        loadFlag.Finished = false;
        Task.Run(() => ItemPalette.INSTANCE.PrepareData(dataVersion, loadFlag));
        while (!loadFlag.Finished) yield return null;

        // Get resource pack manager...
        var packManager = ResourcePackManager.Instance;

        // Load resource packs...
        packManager.ClearPacks();
        // First add base resources
        ResourcePack basePack = new($"vanilla-{resourceVersion}");
        packManager.AddPack(basePack);
        // Then append overrides
        foreach (var packName in resourceOverrides)
            packManager.AddPack(new(packName));
        // Load valid packs...
        loadFlag.Finished = false;
        Task.Run(() => packManager.LoadPacks(loadFlag,
                (status) => Loom.QueueOnMainThread(() => InfoText.text = status)));
        while (!loadFlag.Finished) yield return null;
        
        // Create a dummy world as provider of block colors
        var world = new World();

        float startTime = Time.realtimeSinceStartup;

        int start = 0, limit = 4096;
        int count = 0, width = 64;
        
        foreach (var pair in packManager.StateModelTable)
        {
            int index = count - start;
            if (index >= 0)
            {
                var state = BlockStatePalette.INSTANCE.StatesTable[pair.Key];

                TestBuildState($"Block [{pair.Key}] {state}", pair.Key, state, pair.Value, 0b111111, world, new((index % width) * 2, 0, (index / width) * 2));
            }

            count++;

            if (count >= start + limit)
                break;
        }

        count = 0; width = 32;

        foreach (var pair in packManager.ItemModelTable)
        {
            int index = count - start;
            if (index >= 0)
            {
                var item = ItemPalette.INSTANCE.ItemsTable[pair.Key];
                var itemStack = new ItemStack(item, 1, null);

                TestBuildItem($"Item [{pair.Key}] {item}", pair.Key, itemStack, pair.Value, new((index % width) * 2, 20, (index / width) * 2));
            }

            count++;

            if (count >= start + limit)
                break;

        }

        InfoText.text = $"Voxel meshes built in {Time.realtimeSinceStartup - startTime} seconds.";
    }

    void Start()
    {
        var overrides = new string[] { "vanilla_fix" };
        string resVersion = "1.16.5", dataVersion = "1.16.5";

        if (!Directory.Exists(PathHelper.GetPackDirectoryNamed($"vanilla-{resVersion}"))) // Prepare resources first
        {
            Debug.Log($"Resources for {resVersion} not present. Downloading...");

            StartCoroutine(ResourceDownloader.DownloadResource(resVersion,
                    (status) => Loom.QueueOnMainThread(() => InfoText.text = status), () => { },
                    (succeeded) => {
                        if (succeeded) // Resources ready, do build
                            StartCoroutine(DoBuild(dataVersion, resVersion, overrides, 16));
                        else // Failed to download resources
                            InfoText.text = $"Failed to download resources for {resVersion}.";
                    }));
        }
        else // Resources ready, do build
            StartCoroutine(DoBuild(dataVersion, resVersion, overrides, 16));

    }

}
