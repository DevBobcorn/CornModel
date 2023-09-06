using System.Globalization;
using CraftSharp.Molang.Parser.Exceptions;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements Float parsing
	/// </summary>
	public class FloatParselet : PrefixParselet
	{
		private const NumberStyles NumberStyle = System.Globalization.NumberStyles.AllowDecimalPoint;
		private static readonly CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
		
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			if (float.TryParse(token.Text, NumberStyle, Culture, out var result))
			{
				return new NumberExpression(result);
			}

			throw new MoLangParserException($"Could not parse \'{token.Text.ToString()}\' as float");
		}
	}
}