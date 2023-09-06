using System.Threading;

namespace CraftSharp.Molang.Parser.Tokenizer
{
	public sealed class TokenType
	{
		public static readonly TokenType EqualsEquals = new TokenType("==", "EqualsEquals");
		public static readonly TokenType NotEquals = new TokenType("!=", "Not Equals");
		public static readonly TokenType Coalesce = new TokenType("??", "Coalesce");
		public static readonly TokenType And = new TokenType("&&", "AndAnd");
		public static readonly TokenType Or = new TokenType("||", "Or");
		public static readonly TokenType GreaterOrEquals = new TokenType(">=", "Greater or equal to");
		public static readonly TokenType SmallerOrEquals = new TokenType("<=", "Smaller or equal to");
		public static readonly TokenType Arrow = new TokenType("->", "Arrow");

		public static readonly TokenType Greater = new TokenType(">", "Bigger");
		public static readonly TokenType Smaller = new TokenType("<", "Smaller");
		public static readonly TokenType BracketLeft = new TokenType("(", "Bracket Left");
		public static readonly TokenType BracketRight = new TokenType(")", "Bracket Right");
		public static readonly TokenType ArrayLeft = new TokenType("[", "Array Left");
		public static readonly TokenType ArrayRight = new TokenType("]", "Array Right");
		public static readonly TokenType CurlyBracketLeft = new TokenType("{", "Curly Bracket Left");
		public static readonly TokenType CurlyBracketRight = new TokenType("}", "Curly Bracket Right");
		public static readonly TokenType Comma = new TokenType(",", "Comma");
		public static readonly TokenType Assign = new TokenType("=", "Assign");
		public static readonly TokenType Plus = new TokenType("+", "Plus");
		public static readonly TokenType Minus = new TokenType("-", "Minus");
		public static readonly TokenType Asterisk = new TokenType("*", "Asterisk");
		public static readonly TokenType Slash = new TokenType("/", "Slash");
		public static readonly TokenType Question = new TokenType("?", "Question");
		public static readonly TokenType Colon = new TokenType(":", "Colon");
		public static readonly TokenType Semicolon = new TokenType(";", "Semicolon");
		public static readonly TokenType Bang = new TokenType("!", "Bang");

		public static readonly TokenType Return = new TokenType("return", "Return");
		public static readonly TokenType Continue = new TokenType("continue", "Continue");
		public static readonly TokenType Break = new TokenType("break", "Break");
		public static readonly TokenType ForEach = new TokenType("for_each", "ForEach");
		public static readonly TokenType Loop = new TokenType("loop", "Loop");
		public static readonly TokenType This = new TokenType("this", "Reference");
		public static readonly TokenType True = new TokenType("true", "bool");
		public static readonly TokenType False = new TokenType("false", "bool");
		public static readonly TokenType String = new TokenType("", "string");
		public static readonly TokenType Number = new TokenType("", "number");
		public static readonly TokenType FloatingPointNumber = new TokenType("", "floating point number");
		public static readonly TokenType Name = new TokenType("", "name");
		public static readonly TokenType Eof = new TokenType("", "EndOfFile");

		private static int _typeCounter = 0;

		/// <summary>
		///		A string "identifying" this tokentype
		/// </summary>
		public string Symbol { get; }
		
		/// <summary>
		///		A human readable identifier for this tokentype
		/// </summary>
		public string TypeName { get; }

		private readonly int _typeId;

		private TokenType(string symbol, string typename = "")
		{
			Symbol = symbol;
			TypeName = typename;

			_typeId = Interlocked.Increment(ref _typeCounter);
		}

		private static readonly TokenType[] Values = new TokenType[]
		{
			EqualsEquals, NotEquals, Coalesce, And, Or, GreaterOrEquals, SmallerOrEquals, Arrow, Greater, Smaller,
			BracketLeft, BracketRight, ArrayLeft, ArrayRight, CurlyBracketLeft, CurlyBracketRight, Comma, Assign,
			Plus, Minus, Asterisk, Slash, Question, Colon, Semicolon, Bang, Return, Continue, Break, ForEach, Loop,
			This, True, False, String, Number, Name, Eof
		};

		/// <summary>
		///		Finds the TokenType by symbol
		/// </summary>
		/// <param name="symbol">The symbol to lookup</param>
		/// <returns>The resulting TokenType or null if no match has been found</returns>
		public static TokenType BySymbol(string symbol)
		{
			foreach (TokenType tokenType in TokenType.Values)
			{
				if (tokenType.Symbol.Equals(symbol))
				{
					return tokenType;
				}
			}

			return null;
		}
		
		/// <summary>
		///		Finds the TokenType by symbol
		/// </summary>
		/// <param name="symbol">The symbol to lookup</param>
		/// <returns>The resulting TokenType or null if no match has been found</returns>
		public static TokenType BySymbol(char symbol)
		{
			return BySymbol(symbol.ToString());
		}

		/// <summary>
		///		Checks if this tokentype is equal to <param name="other"/>
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool Equals(TokenType other)
		{
			return _typeId == other._typeId;
		}
	}
}