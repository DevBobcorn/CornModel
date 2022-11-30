using System.Collections.Generic;

namespace MinecraftClient.Resource
{
    public class ItemModel
    {
        public readonly ItemGeometry[] Geometries;

        public ItemModel(List<ItemGeometry> geometries)
        {
            Geometries = geometries.ToArray();
        }

    }

}