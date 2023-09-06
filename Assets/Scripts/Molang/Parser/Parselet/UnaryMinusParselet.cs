using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the unary minus parser
	/// </summary>
	public class UnaryMinusParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new UnaryMinusExpression(parser.ParseExpression(Precedence.Prefix));
		}
	}
}