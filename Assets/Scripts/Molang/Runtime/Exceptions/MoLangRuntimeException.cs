using System;
using System.Text;
using CraftSharp.Molang.Parser;

namespace CraftSharp.Molang.Runtime.Exceptions
{
	/// <summary>
	///		Represents an error that occured during the execution of a MoLang expression
	/// </summary>
	public class MoLangRuntimeException : Exception
	{
		public string MolangTrace { get; }

		/// <summary>
		///		Initializes a new instance of the MoLangRuntimeException class with a specified error message.
		/// </summary>
		/// <param name="message">The error message</param>
		public MoLangRuntimeException(string message) : this(message, null)
		{
			
		}
		
		/// <summary>
		///		Initializes a new instance of the MoLangRuntimeException class with a specified error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="message">The error message</param>
		/// <param name="baseException">A reference to the inner exception that is the cause of this exception</param>
		public MoLangRuntimeException(string message, Exception baseException) : base(message, baseException)
		{
			MolangTrace = "Unknown";
		}
		
		/// <summary>
		///		Initializes a new instance of the MoLangRuntimeException class with a reference to the expression that the error occured at, an error message and a reference to the inner exception that is the cause of this exception.
		/// </summary>
		/// <param name="expression">The expression that this exception occured at</param>
		/// <param name="message">The error message</param>
		/// <param name="baseException">A reference to the inner exception that is the cause of this exception</param>
		public MoLangRuntimeException(IExpression expression, string message, Exception baseException) : base(
			message, baseException)
		{
			StringBuilder sb = new StringBuilder();

			do
			{
				if (expression.Meta?.Token?.Position != null)
				{
					var token = expression.Meta.Token;
					var tokenPosition = token.Position;

					sb.Append(
						$"at <{tokenPosition.LineNumber}:{tokenPosition.Index}> near {token.Type.TypeName} \"{token.Text}\"");
					//var frame = new StackFrame(null, tokenPosition.LineNumber, tokenPosition.Index);

					//	frames.Add(frame);
				}

				expression = expression.Meta.Parent;
			} while (expression?.Meta?.Parent != null);

			MolangTrace = sb.ToString();
			//st.GetFrames()
		}

		//private IEnumerable<IExpression> 
	}
}