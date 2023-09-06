using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions.BinaryOp
{
	public class GreaterOrEqualExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public GreaterOrEqualExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(
				Left.Evaluate(scope, environment).AsDouble() >= Right.Evaluate(scope, environment).AsDouble());
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return ">=";
		}
	}
}