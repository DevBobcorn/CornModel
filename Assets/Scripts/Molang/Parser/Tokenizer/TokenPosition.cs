namespace CraftSharp.Molang.Parser.Tokenizer
{
	/// <summary>
	///		Describes the position of a <see cref="Token"/>
	/// </summary>
	public class TokenPosition
	{
		//	public int StartLineNumber;
		//public int EndLineNumber;
		//public int StartColumn;
		//	public int  EndColumn;
		/// <summary>
		///		The linenumber this token was found at
		/// </summary>
		public int LineNumber { get; }
		
		/// <summary>
		///		The character index of this token
		/// </summary>
		public int Index { get; }

		public TokenPosition(int lastStepLine, int currentLine, int lastStep, int index)
		{
			LineNumber = currentLine;
			Index = index;
		}
	}
}