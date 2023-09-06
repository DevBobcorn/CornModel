using System;
using System.Collections.Generic;
using System.Diagnostics;
using CraftSharp.Molang.Parser.Exceptions;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Parser.Parselet;
using CraftSharp.Molang.Parser.Tokenizer;
using CraftSharp.Molang.Parser.Visitors;
using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser
{
	/// <summary>
	///		The parser used to parse MoLang expressions
	/// </summary>
	public class MoLangParser
	{
		public delegate MoLangParser ParserFactory(ITokenIterator iterator);
		
		/// <summary>
		///		The factory method used to provide instances of the <see cref="MoLangParser"/>
		/// </summary>
		public static ParserFactory Factory { get; set; } = DefaultFactory;

		public static TimeSpan TotalTimeSpent { get; private set; } = TimeSpan.Zero;

		private static readonly Dictionary<TokenType, PrefixParselet> PrefixParselets =
			new Dictionary<TokenType, PrefixParselet>();

		private static readonly Dictionary<TokenType, InfixParselet> InfixParselets =
			new Dictionary<TokenType, InfixParselet>();

		static MoLangParser()
		{
			PrefixParselets.Add(TokenType.Name, new NameParselet());
			PrefixParselets.Add(TokenType.String, new StringParselet());
			PrefixParselets.Add(TokenType.Number, new NumberParselet());
			PrefixParselets.Add(TokenType.FloatingPointNumber, new FloatParselet());
			PrefixParselets.Add(TokenType.True, new BooleanParselet());
			PrefixParselets.Add(TokenType.False, new BooleanParselet());
			PrefixParselets.Add(TokenType.Return, new ReturnParselet());
			PrefixParselets.Add(TokenType.Continue, new ContinueParselet());
			PrefixParselets.Add(TokenType.Break, new BreakParselet());
			PrefixParselets.Add(TokenType.Loop, new LoopParselet());
			PrefixParselets.Add(TokenType.ForEach, new ForEachParselet());
			PrefixParselets.Add(TokenType.This, new ThisParselet());
			PrefixParselets.Add(TokenType.BracketLeft, new GroupParselet());
			PrefixParselets.Add(TokenType.CurlyBracketLeft, new BracketScopeParselet());
			PrefixParselets.Add(TokenType.Minus, new UnaryMinusParselet());
			PrefixParselets.Add(TokenType.Plus, new UnaryPlusParselet());
			PrefixParselets.Add(TokenType.Bang, new BooleanNotParselet());

			InfixParselets.Add(TokenType.Question, new TernaryParselet());
			InfixParselets.Add(TokenType.ArrayLeft, new ArrayAccessParselet());
			InfixParselets.Add(TokenType.Plus, new GenericBinaryOpParselet(Precedence.Sum));
			InfixParselets.Add(TokenType.Minus, new GenericBinaryOpParselet(Precedence.Sum));
			InfixParselets.Add(TokenType.Slash, new GenericBinaryOpParselet(Precedence.Product));
			InfixParselets.Add(TokenType.Asterisk, new GenericBinaryOpParselet(Precedence.Product));
			InfixParselets.Add(TokenType.EqualsEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.NotEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.Greater, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.GreaterOrEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.Smaller, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.SmallerOrEquals, new GenericBinaryOpParselet(Precedence.Compare));
			InfixParselets.Add(TokenType.And, new GenericBinaryOpParselet(Precedence.And));
			InfixParselets.Add(TokenType.Or, new GenericBinaryOpParselet(Precedence.Or));
			InfixParselets.Add(TokenType.Coalesce, new GenericBinaryOpParselet(Precedence.Coalesce));
			InfixParselets.Add(TokenType.Arrow, new GenericBinaryOpParselet(Precedence.Arrow));
			InfixParselets.Add(TokenType.Assign, new AssignParselet());
		}
		
		/// <summary>
		///		The expression traverser executed after the parsing of the expressions
		/// </summary>
		public ExpressionTraverser ExpressionTraverser { get; }
		
		private readonly ITokenIterator _tokenIterator;
		private readonly List<Token> _readTokens = new List<Token>();
		public MoLangParser(ITokenIterator iterator)
		{
			_tokenIterator = iterator;
			ExpressionTraverser = new ExpressionTraverser();
		}

		/// <summary>
		///		Parses the tokens as provided by the <see cref="TokenIterator"/>
		/// </summary>
		/// <returns>An expression that can be executed by an instance of <see cref="MoLangRuntime"/></returns>
		/// <exception cref="MoLangParserException"></exception>
		public IExpression Parse()
		{
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				List<IExpression> exprs = new List<IExpression>();

				do
				{
					IExpression expr = ParseExpression();

					if (expr != null)
					{
						exprs.Add(expr);
					}
					else
					{
						break;
					}
				} while (MatchToken(TokenType.Semicolon));

				var result = ExpressionTraverser.Traverse(exprs.ToArray());
				if (result.Length > 1)
				{
					return new ScriptExpression(result);
				}
				else if (result.Length == 1)
				{
					return result[0];
				}

				return null;
			}
			catch (Exception ex)
			{
				throw new MoLangParserException($"Failed to parse expression", ex);
			}
			finally
			{
				sw.Stop();
				TotalTimeSpent += sw.Elapsed;
			}
		}

		internal IExpression ParseExpression()
		{
			return ParseExpression(Precedence.Anything);
		}

		internal IExpression ParseExpression(Precedence precedence)
		{
			Token token = ConsumeToken();

			if (token.Type.Equals(TokenType.Eof))
			{
				return null;
			}

			PrefixParselet parselet = PrefixParselets[token.Type];

			if (parselet == null)
			{
				throw new MoLangParserException("Cannot parse " + token.Type.GetType().Name + " expression");
			}

			IExpression expr = parselet.Parse(this, token);
			InitExpr(expr, token);
			
			return ParseInfixExpression(expr, precedence);
		}

		private IExpression ParseInfixExpression(IExpression left, Precedence precedence)
		{
			Token token;

			while (precedence < GetPrecedence())
			{
				token = ConsumeToken();

				if (token.Type == TokenType.Eof)
					return left;

				if (!InfixParselets.TryGetValue(token.Type, out var infixParselet))
				{
					throw new MoLangParserException(
						$"Invalid infix token of type '{token.Type.TypeName}' and text '{token.Text}' at {token.Position.LineNumber}:{token.Position.Index}");
				}

				left = infixParselet.Parse(this, token, left);
				InitExpr(left, token);
			}

			return left;
		}

		private void InitExpr(IExpression expression, Token token)
		{
			expression.Meta.Token = token;
		}

		private Precedence GetPrecedence()
		{
			Token token = ReadToken();

			if (token != null)
			{
				if (token.Type == TokenType.Eof)
					return Precedence.Anything;
				//	InfixParselet parselet = InfixParselets[token.Type];

				if (InfixParselets.TryGetValue(token.Type, out var parselet))
				{
					return parselet.Precedence;
				}
				else
				{
					//throw new MoLangParserException($"Invalid precedence token of type '{token.Type.TypeName}' and text '{token.Text}' at {token.Position.LineNumber}:{token.Position.Index}");
				}
			}

			return Precedence.Anything;
		}

		internal bool TryParseArgs(out IExpression[] expressions)
		{
			expressions = null;
		

			if (!MatchToken(TokenType.BracketLeft)) return false;
			if (MatchToken(TokenType.BracketRight))
			{
				expressions = Array.Empty<IExpression>();
				return true;
			}
			
			List<IExpression> args = new List<IExpression>();
			do
			{
				args.Add(ParseExpression());
			} while (MatchToken(TokenType.Comma));

			ConsumeToken(TokenType.BracketRight);

			expressions = args.ToArray();
			return true;
		}

		internal MoPath FixNameShortcut(MoPath name)
		{
			var first = name.Value;

			switch (first)
			{
				case "q":
					first = "query";

					break;

				case "v":
					first = "variable";

					break;

				case "t":
					first = "temp";

					break;

				case "c":
					first = "context";

					break;
			}

			name.SetValue(first);
			return name; // String.Join(".", splits);
		}

		private Token ConsumeToken()
		{
			return ConsumeToken(null);
		}

		internal Token ConsumeToken(TokenType expectedType)
		{
			_tokenIterator.Step();
			if (!TryReadToken(expectedType, out var token))
			{
				throw new MoLangParserException(
					$"Expected token of type '{expectedType.TypeName}' but found '{token.Type.TypeName}' at line {token.Position.LineNumber}:{token.Position.Index}");
			}

			if (_readTokens.Count > 0)
			{
				_readTokens.RemoveAt(0);

				return token;
			}

			return null;
		}

		internal bool MatchToken(TokenType expectedType)
		{
			return MatchToken(expectedType, true);
		}

		internal bool MatchToken(TokenType expectedType, bool consume)
		{
			if (!TryReadToken(expectedType, out _))
				return false;
			
			if (consume)
				ConsumeToken();

			return true;
		}

		private bool TryReadToken(TokenType expectedType, out Token result)
		{
			result = ReadToken();
			if (result == null || (expectedType != null && !result.Type.Equals(expectedType)))
			{
				return false;
			}

			return true;
		}
		
		private Token ReadToken()
		{
			return ReadToken(0);
		}

		private Token ReadToken(int distance)
		{
			while (distance >= _readTokens.Count)
			{
				_readTokens.Add(_tokenIterator.Next());
			}

			return _readTokens[distance];
		}

		/// <summary>
		///		Parses a MoLang Expression using the parser provided by the <see cref="Factory"/>
		/// </summary>
		/// <param name="input">The MoLang expression to parse</param>
		/// <returns>An array of expressions to be executed by <see cref="MoLangRuntime"/></returns>
		public static IExpression Parse(string input)
		{
			return Factory(new TokenIterator(input)).Parse();
		}
		
		private static MoLangParser DefaultFactory(ITokenIterator iterator)
		{
			var parser = new MoLangParser(iterator);
			parser.ExpressionTraverser.Visitors.Add(new MathOptimizationVisitor());
			
			return parser;
		}
	}
}