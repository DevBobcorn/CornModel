using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Rendering;
using Unity.Collections;
using Unity.Mathematics;
using TMPro;

using CraftSharp.Resource;
using CraftSharp.Molang.Runtime;

namespace CraftSharp.Demo
{
    public class Test : MonoBehaviour
    {
        public const int WINDOWED_APP_WIDTH = 1600, WINDOWED_APP_HEIGHT = 900;
        private static readonly byte[] FLUID_HEIGHTS = new byte[] { 15, 15, 15, 15, 15, 15, 15, 15, 15 };

        private static readonly float3 ITEM_CENTER = new(-0.5F, -0.5F, -0.5F);

        [SerializeField] public TMP_Text InfoText;
        [SerializeField] public Animator CrosshairAnimator;
        [SerializeField] private MaterialManager materialManager;

        [SerializeField] public RectTransform inventory;
        [SerializeField] public GameObject inventoryItemPrefab;
        [SerializeField] public int[] InventoryBuildList = { };

        private bool loaded = false;

        // Runs before a scene gets loaded
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeApp() => Loom.Initialize();

        private static readonly bool[] DUMMY_AO_OCCLUSSION = Enumerable.Repeat(false, 27).ToArray();
        private static readonly float[] DUMMY_BLOCK_VERT_LIGHT = Enumerable.Repeat(0F, 8).ToArray();

        public void TestBuildState(string name, int stateId, BlockState state, BlockStateModel stateModel, int cullFlags, World world, float3 pos)
        {
            int altitude = 0;
            foreach (var model in stateModel.Geometries)
            {
                var coord = pos + new float3(0F, -altitude * 1.2F, 0F);

                var modelObject = new GameObject(name);
                modelObject.transform.parent = transform;
                modelObject.transform.localPosition = coord;

                var filter = modelObject.AddComponent<MeshFilter>();
                var render = modelObject.AddComponent<MeshRenderer>();

                var collider = modelObject.AddComponent<MeshCollider>();

                // Make and set mesh...
                var visualBuffer = new VertexBuffer();

                if (state.InWater)
                    FluidGeometry.Build(ref visualBuffer, float3.zero, FluidGeometry.LiquidTextures[0], FLUID_HEIGHTS,
                            cullFlags, DUMMY_BLOCK_VERT_LIGHT, world.GetWaterColor(BlockLoc.Zero));
                else if (state.InLava)
                    FluidGeometry.Build(ref visualBuffer, float3.zero, FluidGeometry.LiquidTextures[1], FLUID_HEIGHTS,
                            cullFlags, DUMMY_BLOCK_VERT_LIGHT, BlockGeometry.DEFAULT_COLOR);

                int fluidVertexCount = visualBuffer.vert.Length;
                int fluidTriIdxCount = (fluidVertexCount / 2) * 3;

                var color = BlockStatePalette.INSTANCE.GetBlockColor(stateId, world, BlockLoc.Zero, state);
                model.Build(ref visualBuffer, float3.zero, cullFlags, DUMMY_AO_OCCLUSSION,
                        DUMMY_BLOCK_VERT_LIGHT, color);

                int vertexCount = visualBuffer.vert.Length;
                int triIdxCount = (vertexCount / 2) * 3;

                var meshDataArr = Mesh.AllocateWritableMeshData(1);
                var meshData = meshDataArr[0];

                var vertAttrs = new NativeArray<VertexAttributeDescriptor>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
                vertAttrs[0] = new(VertexAttribute.Position,  dimension: 3, stream: 0);
                vertAttrs[1] = new(VertexAttribute.TexCoord0, dimension: 3, stream: 1);
                vertAttrs[2] = new(VertexAttribute.TexCoord3, dimension: 4, stream: 2);
                vertAttrs[3] = new(VertexAttribute.Color,     dimension: 4, stream: 3);

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
                var vertColors = meshData.GetVertexData<float4>(3);
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

                // Create and assign mesh
                var mesh = new Mesh { bounds = bounds };
                Mesh.ApplyAndDisposeWritableMeshData(meshDataArr, mesh);
                // Recalculate mesh normals
                mesh.RecalculateNormals();
                filter.sharedMesh   = mesh;
                collider.sharedMesh = mesh;

                if (state.InWater)
                {
                    render.sharedMaterials =
                        new []{
                            materialManager.GetAtlasMaterial(RenderType.WATER),
                            materialManager.GetAtlasMaterial(stateModel.RenderType)
                        };
                }
                else
                    render.sharedMaterial = materialManager.GetAtlasMaterial(stateModel.RenderType);

                altitude += 1;
            }
        }

        public void TestBuildItem(string name, int itemNumId, ItemStack itemStack, ItemModel itemModel, float3 pos)
        {
            // Gather all geometries of this model
            Dictionary<ItemModelPredicate, ItemGeometry> buildDict = new()
            {
                { ItemModelPredicate.EMPTY, itemModel.Geometry }
            };
            foreach (var pair in itemModel.Overrides)
                buildDict.TryAdd(pair.Key, pair.Value);

            int altitude = 0;
            foreach (var pair in buildDict)
            {
                var coord = pos + new float3(0F, -altitude * 1.2F, 0F);

                var modelObject = new GameObject(pair.Key == ItemModelPredicate.EMPTY ? name : $"{name}{pair.Key}");
                modelObject.transform.parent = transform;
                modelObject.transform.localPosition = coord;

                var filter = modelObject.AddComponent<MeshFilter>();
                var render = modelObject.AddComponent<MeshRenderer>();

                var collider = modelObject.AddComponent<MeshCollider>();

                // Make and set mesh...
                var visualBuffer = new VertexBuffer();

                float3[] colors;

                var tintFunc = ItemPalette.INSTANCE.GetTintRule(itemStack.ItemType.ItemId);
                if (tintFunc is null)
                    colors = new float3[]{ new(1F, 0F, 0F), new(0F, 0F, 1F), new(0F, 1F, 0F) };
                else
                    colors = tintFunc.Invoke(itemStack);

                pair.Value.Build(ref visualBuffer, float3.zero, colors);

                var mesh = VertexBufferBuilder.BuildMesh(visualBuffer);
                filter.sharedMesh   = mesh;
                collider.sharedMesh = mesh;
                render.sharedMaterial = materialManager.GetAtlasMaterial(itemModel.RenderType);

                altitude += 1;
            }
        }

        public void TestBuildInventoryItem(string name, ItemStack itemStack, ItemModel itemModel)
        {
            var invItemObj = GameObject.Instantiate(inventoryItemPrefab);
            invItemObj.name = name;
            invItemObj.GetComponent<RectTransform>().SetParent(inventory, false);

            var filter = invItemObj.GetComponentInChildren<MeshFilter>();
            var modelObject = filter.gameObject;

            var render = modelObject.GetComponent<MeshRenderer>();
            var itemGeometry = itemModel.Geometry;

            // Handle GUI display transform
            bool hasGUITransform = itemGeometry.DisplayTransforms.TryGetValue(DisplayPosition.GUI, out float3x3 t);
            // Make use of the debug text
            invItemObj.GetComponentInChildren<TMP_Text>().text = hasGUITransform ? $"{t.c1.x} {t.c1.y} {t.c1.z}" : string.Empty;

            if (hasGUITransform) // Apply specified local transform
            {
                // Apply local translation, '1' in translation field means 0.1 unit in local space, so multiply with 0.1
                modelObject.transform.localPosition = t.c0 * 0.1F;
                // Apply local rotation
                modelObject.transform.localEulerAngles = Vector3.zero;
                // - MC ROT X
                modelObject.transform.Rotate(Vector3.back, t.c1.x, Space.Self);
                // - MC ROT Y
                modelObject.transform.Rotate(Vector3.down, t.c1.y, Space.Self);
                // - MC ROT Z
                modelObject.transform.Rotate(Vector3.left, t.c1.z, Space.Self);
                // Apply local scale
                modelObject.transform.localScale = t.c2;
            }
            else // Apply uniform local transform
            {
                // Apply local translation, set to zero
                modelObject.transform.localPosition = Vector3.zero;
                // Apply local rotation
                modelObject.transform.localEulerAngles = Vector3.zero;
                // Apply local scale
                modelObject.transform.localScale = Vector3.one;
            }

            // Make and set mesh...
            var visualBuffer = new VertexBuffer();

            float3[] colors;

            var tintFunc = ItemPalette.INSTANCE.GetTintRule(itemStack.ItemType.ItemId);
            if (tintFunc is null)
                colors = new float3[]{ new(1F, 0F, 0F), new(0F, 0F, 1F), new(0F, 1F, 0F) };
            else
                colors = tintFunc.Invoke(itemStack);

            itemGeometry.Build(ref visualBuffer, ITEM_CENTER, colors);

            filter.sharedMesh = VertexBufferBuilder.BuildMesh(visualBuffer);
            // Use material variants with fog disabled
            render.sharedMaterial = materialManager.GetAtlasMaterial(itemModel.RenderType, true);
        }

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

            // Loading complete!
            loaded = true;
            
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

                    TestBuildState($"Block [{pair.Key}] {state}", pair.Key, state, pair.Value, 0b111111, world, new((index % width) * 1.2F, 0, (index / width) * 1.2F));
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

                    TestBuildItem($"Item [{pair.Key}] {item}", pair.Key, itemStack, pair.Value, new(-(index % width) * 1.5F - 1.5F, 0F, (index / width) * 1.5F));
                }

                count++;

                if (count >= start + limit)
                    break;

            }

            InfoText.text = $"Meshes built in {Time.realtimeSinceStartup - startTime} second(s).";
        }

        private void TestAnimation(string jsonStr)
        {
            var data = Json.ParseJson(jsonStr);
            var anim = EntityBoneAnimation.FromJson(data);

            foreach (var keyframe in anim.scaleKeyframes)
            {
                Debug.Log($"SCALE [{keyframe.time}] {keyframe.pre} {keyframe.post}");
            }

            var scope = new MoScope(new());
            var env = new MoLangEnvironment();

            for (float a = -1F; a < 3F; a += 0.05F)
            {
                var val = anim.Evaluate(a, scope, env);
                Debug.Log($"[{a}] => [{val.trans} | {val.scale} {val.rot}]");
            }
        }

        private IEnumerator DoEntityBuild()
        {
            var entityResPath = PathHelper.GetPackDirectoryNamed("bedrock_res");
            var playerModelsPath = PathHelper.GetPackDirectoryNamed("player_models");

            var entityResManager = new EntityResourceManager(entityResPath, playerModelsPath);

            yield return StartCoroutine(entityResManager.LoadEntityResources(new(),
                    (status) => Loom.QueueOnMainThread(() => InfoText.text = status)));
            
            var testmentObj = new GameObject("[Entity Testment]");
            int entityPerRow = 10;
            int index = 0;
            foreach (var pair in entityResManager.EntityDefinitions)
            {
                var entityType = pair.Key;
                var entityDef = pair.Value;

                int i = index / entityPerRow;
                int j = index % entityPerRow;

                var entityRenderObj = new GameObject($"{index} {entityType}");
                entityRenderObj.transform.SetParent(testmentObj.transform);
                entityRenderObj.transform.localPosition= new(i * 2, 0, - (entityPerRow - j) * 2);

                var entityRender = entityRenderObj.AddComponent<EntityModelRender>();
                try
                {
                    entityRender.SetDefinitionData(entityDef);
                    entityRender.BuildEntityModel(entityResManager, materialManager);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"An exception occurred building model for entity {entityType}: {e}");
                }
                
                index++;
            }
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
            {
                StartCoroutine(DoBuild(dataVersion, resVersion, overrides, 16));
            }

            StartCoroutine(DoEntityBuild());

            IsPaused = false;
        }

        private bool isPaused = false;
        public bool IsPaused
        {
            get => isPaused;
            set {
                isPaused = value;
                // Update cursor lock
                Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
                // Update crosshair visibility
                CrosshairAnimator.SetBool("Show", !value);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                IsPaused = !IsPaused;
            }

            if (Input.GetKeyDown(KeyCode.Q)) // Rebuild inventory items
            {
                if (!loaded)
                {
                    Debug.LogWarning($"Resource loading in progress, please wait...");
                    return;
                }

                var items = new List<Transform>();
                foreach (Transform item in inventory.transform)
                {
                    items.Add(item);
                }
                
                foreach (var item in items)
                {
                    Destroy(item.gameObject);
                }

                var packManager = ResourcePackManager.Instance;

                foreach (var itemNumId in InventoryBuildList)
                {
                    var item = ItemPalette.INSTANCE.ItemsTable[itemNumId];
                    var itemStack = new ItemStack(item, 1, null);

                    TestBuildInventoryItem($"Item [{itemNumId}] {item}", itemStack, packManager.ItemModelTable[itemNumId]);
                }
            }

            if (Input.GetKeyDown(KeyCode.F11)) // Toggle full screen
            {
                if (Screen.fullScreen)
                {
                    Screen.SetResolution(WINDOWED_APP_WIDTH, WINDOWED_APP_HEIGHT, false);
                    Screen.fullScreen = false;
                }
                else
                {
                    var maxRes = Screen.resolutions[Screen.resolutions.Length - 1];
                    Screen.SetResolution(maxRes.width, maxRes.height, true);
                    Screen.fullScreen = true;
                }
            }
        }
    }
}