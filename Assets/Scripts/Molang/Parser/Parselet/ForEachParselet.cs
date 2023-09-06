using CraftSharp.Molang.Parser.Exceptions;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "foreach" instruction parser
	/// </summary>
	public class ForEachParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			if (!parser.TryParseArgs(out var expressions) || expressions.Length != 3)
				throw new MoLangParserException($"ForEach: Expected 3 argument, {(expressions?.Length ?? 0)} argument given");
			
			return new ForEachExpression(expressions[0], expressions[1], expressions[2]);
		}
	}
}