using UnityEngine;
using MinecraftClient;
using MinecraftClient.Resource;
using MinecraftClient.Mapping;
using MinecraftClient.Mapping.BlockStatePalettes;
public class Test : MonoBehaviour
{
    public void TestBuildModel(string name, BlockModel model, int cullFlags, Vector3 pos)
    {
        // First prepare our model data
        var wrapper = new BlockModelWrapper(model, Vector2Int.zero, false);
        var geometry = new BlockGeometry(wrapper);
        var geoData = geometry.GetData(cullFlags);

        var modelObject = new GameObject(name);
        modelObject.transform.parent = transform;
        modelObject.transform.localPosition = pos;

        var filter = modelObject.AddComponent<MeshFilter>();
        var render = modelObject.AddComponent<MeshRenderer>();

        // Make and set mesh...
        var mesh = new Mesh();

        // Build things up!
        mesh.vertices = geoData.Item1;
        mesh.uv = geoData.Item2;
        mesh.triangles = geoData.Item3;

        filter.sharedMesh = mesh;

        render.sharedMaterial = BlockTextureManager.atlasMaterial;

    }

    public void TestUVLock(string name, BlockModel model, int cullFlags, Vector3 pos)
    {
        for (int yrot = 0;yrot < 4;yrot++)
            for (int zrot = 0;zrot < 4;zrot++)
            {
                // First prepare our model data
                var wrapper = new BlockModelWrapper(model, new Vector2Int(zrot, yrot), false);
                var geometry = new BlockGeometry(wrapper);
                var geoData = geometry.GetData(cullFlags);

                var modelObject = new GameObject(name + " yr=" + yrot + ", zr=" + zrot);
                modelObject.transform.parent = transform;
                modelObject.transform.localPosition = pos + new Vector3(zrot * 2, 0, yrot * 2);

                var filter = modelObject.AddComponent<MeshFilter>();
                var render = modelObject.AddComponent<MeshRenderer>();

                // Make and set mesh...
                var mesh = new Mesh();

                // Build things up!
                mesh.vertices = geoData.Item1;
                mesh.uv = geoData.Item2;
                mesh.triangles = geoData.Item3;

                filter.sharedMesh = mesh;

                render.sharedMaterial = BlockTextureManager.atlasMaterial;
            }
    }

    public void TestBuildState(string name, BlockGeometry geometry, int cullFlags, Vector3 pos)
    {
        // First prepare our model data
        var geoData = geometry.GetData(cullFlags);

        var modelObject = new GameObject(name);
        modelObject.transform.parent = transform;
        modelObject.transform.localPosition = pos;

        var filter = modelObject.AddComponent<MeshFilter>();
        var render = modelObject.AddComponent<MeshRenderer>();

        // Make and set mesh...
        var mesh = new Mesh();

        // Build things up!
        mesh.vertices = geoData.Item1;
        mesh.uv = geoData.Item2;
        mesh.triangles = geoData.Item3;

        filter.sharedMesh = mesh;

        render.sharedMaterial = BlockTextureManager.atlasMaterial;

    }

    void Start()
    {
        // First load all possible Block States...
        Block.Palette = new Palette116();
        Block.Palette.ReadBlockStates();

        // Create a new resource pack...
        ResourcePackManager manager = new ResourcePackManager();
        ResourcePack pack = new ResourcePack("vanilla-1.16.5");
        
        // Load resource pack if valid...
        if (pack.IsValid)
        {
            manager.AddPack(pack);
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

                    int height = 0;

                    foreach (var geo in item.Value)
                    {
                        if (geo != null)
                            TestBuildState(stateName, geo, 0b111111, new Vector3((index % width) * 2, height, (index / width) * 2));
                        height -= 2;
                    }
                }

                count++;

                if (count >= start + limit)
                    break;

            }

            TestUVLock("UVLock Test Unit ", manager.modelsTable[ResourceLocation.fromString("block/dir_block")], 0b111111, new Vector3(0, 3, 0));

            Debug.Log("Unity meshes built in " + (Time.realtimeSinceStartup - startTime) + " seconds.");

        }

    }
}
