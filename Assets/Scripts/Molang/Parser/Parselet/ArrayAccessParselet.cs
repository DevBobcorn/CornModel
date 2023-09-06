using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the indexer/array accessor parser
	/// </summary>
	/// <remarks>
	///		Parses expressions such as "array[0]"
	/// </remarks>
	public class ArrayAccessParselet : InfixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr)
		{
			IExpression index = parser.ParseExpression(Precedence);
			parser.ConsumeToken(TokenType.ArrayRight);

			return new ArrayAccessExpression(leftExpr, index);
		}

		/// <inheritdoc />
		public ArrayAccessParselet() : base(Precedence.ArrayAccess) { }
	}
}