using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class UnaryMinusExpression : Expression
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(-(Parameters[0].Evaluate(scope, environment).AsDouble()));
		}

		/// <inheritdoc />
		public UnaryMinusExpression(IExpression value) : base(value)
		{
			
		}
	}
}