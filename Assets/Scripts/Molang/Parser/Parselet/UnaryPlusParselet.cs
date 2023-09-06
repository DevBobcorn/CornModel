using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the unary plus parser
	/// </summary>
	public class UnaryPlusParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new UnaryPlusExpression(parser.ParseExpression(Precedence.Prefix));
		}
	}
}