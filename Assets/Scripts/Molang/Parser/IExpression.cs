using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser
{
	/// <summary>
	///		The interface for all expressions to implement
	/// </summary>
	public interface IExpression
	{
		/// <summary>
		///		Contains metadata about this expression
		/// </summary>
		ExpressionMeta Meta { get; }

		/// <summary>
		///		Evaluate the expression
		/// </summary>
		/// <param name="scope">The current scope</param>
		/// <param name="environment">The environment provided by the <see cref="MoLangRuntime"/></param>
		/// <returns>The value returned by the expression</returns>
		IMoValue Evaluate(MoScope scope, MoLangEnvironment environment);

		/// <summary>
		///		Invoked when trying to assign a value to a property/field
		/// </summary>
		/// <param name="scope">The current scope</param>
		/// <param name="environment">The environment provided by the <see cref="MoLangRuntime"/></param>
		/// <param name="value">The value to assign</param>
		void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value);

		/// <summary>
		///		The parameters used by this expression
		/// </summary>
		IExpression[] Parameters { get; set; }
	}

	public abstract class Expression<T> : Expression
	{
		protected Expression(T value) : base()
		{
			Value = value;
		}

		public T Value { get; set; }
	}
}