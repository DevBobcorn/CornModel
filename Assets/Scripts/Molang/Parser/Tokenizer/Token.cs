namespace CraftSharp.Molang.Parser.Tokenizer
{
	/// <summary>
	///		A token is a piece of code in an expression
	/// </summary>
	public class Token
	{
		/// <summary>
		///		The type of this token
		/// </summary>
		public TokenType Type { get; }
		
		/// <summary>
		///		The piece of code defining this token
		/// </summary>
		public string Text { get; }
		
		/// <summary>
		///		The position in the source expression that this token was found at
		/// </summary>
		public TokenPosition Position { get; }

		/// <summary>
		///		Initializes a new token
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="position"></param>
		public Token(TokenType tokenType, TokenPosition position)
		{
			this.Type = tokenType;
			this.Text = tokenType.Symbol;
			this.Position = position;
		}

		/// <summary>
		///		Initializes a new token
		/// </summary>
		/// <param name="tokenType"></param>
		/// <param name="text"></param>
		/// <param name="position"></param>
		public Token(TokenType tokenType, string text, TokenPosition position)
		{
			this.Type = tokenType;
			this.Text = text;
			this.Position = position;
		}
	}
}