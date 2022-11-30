namespace MinecraftClient.Mapping
{
    public class Item
    {
        public static readonly ResourceLocation AIR_ID = new ResourceLocation("air");
        public static readonly Item UNKNOWN  = new Item(new("unknown"));
        public static readonly Item AIR_ITEM = new Item(new("air"));

        public readonly ResourceLocation itemId; // Something like 'minecraft:grass_block'

        public Item(ResourceLocation itemId)
        {
            this.itemId = itemId;
        }

        public override string ToString()
        {
            return itemId.ToString();
        }
    }
}