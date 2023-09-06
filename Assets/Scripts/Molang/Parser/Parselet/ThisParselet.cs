using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "this" instruction parser
	/// </summary>
	public class ThisParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			return new ThisExpression();
		}
	}
}