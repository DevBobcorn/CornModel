using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class ThisExpression : Expression
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment.ThisVariable; // environment.GetValue(_this);
		}

		/// <inheritdoc />
		public ThisExpression() { }
	}
}