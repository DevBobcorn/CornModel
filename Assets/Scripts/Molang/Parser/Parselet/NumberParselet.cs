using System.Globalization;
using CraftSharp.Molang.Parser.Exceptions;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements number parsing
	/// </summary>
	public class NumberParselet : PrefixParselet
	{
		private const NumberStyles NumberStyle = System.Globalization.NumberStyles.AllowDecimalPoint;
		private static readonly CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;

		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			if (double.TryParse(token.Text, NumberStyle, Culture, out var result))
			{
				return new NumberExpression(result);
			}

			throw new MoLangParserException($"Could not parse \'{token.Text.ToString()}\' as a double");
		}
	}
}