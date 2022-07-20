using System.Collections.Generic;
using MinecraftClient.Resource;

namespace MinecraftClient.BlockData
{
    public class BlockState
    {
        public readonly ResourceLocation blockId; // Something like 'minecraft:grass_block'
        public readonly Dictionary<string, string> props;

        public static BlockState fromString(string state)
        {
            var props = new Dictionary<string, string>();

            // Add namespace if absent...
            if (!state.Contains(':'))
                state = state.Insert(0, "minecraft:");
            
            // Assign block state property values
            if (state.Contains('['))
            {
                string[] parts = state.Split('[', 2);
                var blockId = ResourceLocation.fromString(parts[0]);
                string[] propStrs = parts[1].Substring(0, parts[1].Length - 1).Split(',');
                foreach (var prop in propStrs)
                {
                    string[] keyVal = prop.Split('=', 2);
                    props.Add(keyVal[0], keyVal[1]);
                }
                return new BlockState(blockId, props);
            }
            else
            {
                // Simple, only a block id
                return new BlockState(ResourceLocation.fromString(state), props);
            }

        }

        public BlockState(ResourceLocation blockId)
        {
            this.blockId = blockId;
            this.props = new Dictionary<string, string>();
        }

        public BlockState(ResourceLocation blockId, Dictionary<string, string> props)
        {
            this.blockId = blockId;
            this.props = props;
        }

        public override string ToString()
        {
            if (props.Count > 0)
            {
                var prop = props.GetEnumerator();
                prop.MoveNext();
                string propsText = prop.Current.Key + '=' + prop.Current.Value;

                while (prop.MoveNext())
                {
                    propsText += ',' + prop.Current.Key + '=' + prop.Current.Value;
                }

                return blockId.ToString() + '[' + propsText + ']';
            }
            else return blockId.ToString();
        }

    }
}

