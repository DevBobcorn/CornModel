using System.IO;
using UnityEngine;

namespace MinecraftClient.Resource
{
    public class BlockModelLoader
    {
        public static JsonModel INVALID_MODEL = new JsonModel();

        private readonly ResourcePackManager manager;

        public BlockModelLoader(ResourcePackManager manager)
        {
            this.manager = manager;
        }

        // Accepts the assets path of current resource pack so that it can easily find other model
        // files(when searching for a parent model which is not loaded yet, for example)
        public JsonModel LoadBlockModel(ResourceLocation identifier, string assetsPath)
        {
            // Check if this model is loaded already...
            if (manager.blockModelTable.ContainsKey(identifier))
                return manager.blockModelTable[identifier];
            
            string modelPath = assetsPath + '/' + identifier.nameSpace + "/models/" + identifier.path + ".json";
            if (File.Exists(modelPath))
            {
                JsonModel model = new JsonModel();

                string modelText = File.ReadAllText(modelPath);
                Json.JSONData modelData = Json.ParseJson(modelText);

                bool containsTextures = modelData.Properties.ContainsKey("textures");
                bool containsElements = modelData.Properties.ContainsKey("elements");

                if (modelData.Properties.ContainsKey("parent"))
                {
                    ResourceLocation parentIdentifier = ResourceLocation.fromString(modelData.Properties["parent"].StringValue.Replace('\\', '/'));
                    JsonModel parentModel;
                    if (manager.blockModelTable.ContainsKey(parentIdentifier))
                    {
                        // This parent is already loaded, get it...
                        parentModel = manager.blockModelTable[parentIdentifier];
                    }
                    else
                    {
                        // This parent is not yet loaded, load it...
                        parentModel = LoadBlockModel(parentIdentifier, assetsPath);
                    }

                    // Inherit parent textures...
                    foreach (var tex in parentModel.textures)
                    {
                        model.textures.Add(tex.Key, tex.Value);
                    }

                    // Inherit parent elements only if itself doesn't have those defined...
                    if (!containsElements)
                    {
                        foreach (var elem in parentModel.elements)
                        {
                            model.elements.Add(elem);
                        }
                    }
                }

                if (containsTextures) // Add / Override texture references
                {
                    var texData = modelData.Properties["textures"].Properties;
                    foreach (var texItem in texData)
                    {
                        TextureReference texRef;
                        if (texItem.Value.StringValue.StartsWith('#'))
                        {
                            texRef = new TextureReference(true, texItem.Value.StringValue.Substring(1)); // Remove the leading '#'...
                        }
                        else
                        {
                            texRef = new TextureReference(false, texItem.Value.StringValue);
                        }

                        if (model.textures.ContainsKey(texItem.Key)) // Override this texture reference...
                        {
                            model.textures[texItem.Key] = texRef;
                        }
                        else // Add a new texture reference...
                        {
                            model.textures.Add(texItem.Key, texRef);
                        }
                    }

                }

                if (containsElements) // Discard parent elements and use own ones
                {
                    var elemData = modelData.Properties["elements"].DataArray;
                    foreach (var elemItem in elemData)
                    {
                        model.elements.Add(JsonModelElement.fromJson(elemItem));
                    }
                }

                // It's also possible that this model is added somewhere before
                // during parent loading process (though it shouldn't happen)
                if (manager.blockModelTable.TryAdd(identifier, model))
                {
                    //Debug.Log("Model loaded: " + identifier);
                }
                else
                {
                    Debug.LogWarning("Trying to add model twice: " + identifier);
                }

                return model;
            }
            else
            {
                Debug.LogWarning("Block model file not found: " + modelPath);
                return INVALID_MODEL;
            }
        }
    }
}