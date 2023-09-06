using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Tokenizer;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser.Parselet
{
	/// <summary>
	///		Implements the "name" parser
	/// </summary>
	/// <remarks>
	///		Used to parse function calls or property accessors.
	///		For example: "query.frame_time" or "math.min(10, 20)"
	/// </remarks>
	public class NameParselet : PrefixParselet
	{
		/// <inheritdoc />
		public override IExpression Parse(MoLangParser parser, Token token)
		{
			var path = parser.FixNameShortcut(new MoPath(token.Text));

			if (parser.TryParseArgs(out var expressions))
				return new FuncCallExpression(path, expressions);

			return new NameExpression(path);
		}
	}
}