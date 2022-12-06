using System.Collections.Generic;

using MinecraftClient.Mapping;

namespace MinecraftClient.Inventory
{
    /// <summary>
    /// Represents an item stack
    /// Placeholder in this project
    /// </summary>
    public class ItemStack
    {
        public Item Type;

        public int Count;

        #nullable enable
        public Dictionary<string, object>? NBT;

        public ItemStack(Item itemType, int count, Dictionary<string, object>? nbt)
        {
            this.Type = itemType;
            this.Count = count;
            this.NBT = nbt;
        }
        #nullable disable

        public bool IsEmpty
        {
            get
            {
                return Type == Item.AIR_ITEM || Count == 0;
            }
        }

        public string DisplayName { get; }

        public string[] Lores { get; }

        public int Damage { get; } = 0;

        public override string ToString() => $"{Type.itemId} * {Count}";
    }
}
