using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "!=" parselet
	/// </summary>
	public class BooleanNotParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new BooleanNotExpression(parser.ParseExpression(Precedence.Prefix));
		}
	}
}