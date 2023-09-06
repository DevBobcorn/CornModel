using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser
{
	public abstract class InfixParselet
	{
		public Precedence Precedence { get; protected set; }

		public InfixParselet(Precedence precedence)
		{
			Precedence = precedence;
		}

		public abstract IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr);
	}
}