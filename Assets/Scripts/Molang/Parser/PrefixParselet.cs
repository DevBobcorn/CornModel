using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser
{
    public abstract class PrefixParselet
    {
        public abstract IExpression Parse(MoLangParser parser, Token token);
    }
}