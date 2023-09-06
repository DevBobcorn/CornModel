using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "=" parselet
	/// </summary>
	public class AssignParselet : InfixParselet
	{
		/// <inheritdoc />
		public AssignParselet() : base(Precedence.Assignment) { }

		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token, IExpression leftExpr)
		{
			return new AssignExpression(leftExpr, parser.ParseExpression(Precedence));
		}
	}
}