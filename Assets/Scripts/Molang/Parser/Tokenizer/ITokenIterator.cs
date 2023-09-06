namespace CraftSharp.Molang.Parser.Tokenizer
{
    public interface ITokenIterator
    {
        Token Next();
        void Step();
    }
}