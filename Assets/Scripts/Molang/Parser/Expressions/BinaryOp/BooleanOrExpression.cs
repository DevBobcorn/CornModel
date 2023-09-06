using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions.BinaryOp
{
	public class BooleanOrExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public BooleanOrExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return new DoubleValue(
				Left.Evaluate(scope, environment).AsBool() || Right.Evaluate(scope, environment).AsBool());
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "||";
		}
	}
}