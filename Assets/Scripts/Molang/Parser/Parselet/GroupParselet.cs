using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "group" parselet
	/// </summary>
	public class GroupParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			IExpression expr = parser.ParseExpression();
			var result = parser.ConsumeToken(TokenType.BracketRight);

			return expr;
		}
	}
}