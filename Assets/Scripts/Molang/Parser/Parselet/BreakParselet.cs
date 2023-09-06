using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "break" instruction parser
	/// </summary>
	public class BreakParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new BreakExpression();
		}
	}
}