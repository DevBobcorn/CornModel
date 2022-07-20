namespace MinecraftClient.Mapping
{
    public class BlockProp
    {
        public readonly string propName;
        public readonly string propValue;

        public BlockProp(string name, string val)
        {
            propName = name;
            propValue = val;
        }
    }
}
