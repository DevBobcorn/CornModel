using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class TernaryExpression : Expression
	{
		public IExpression Condition => Parameters[0];
		public IExpression ThenExpr => Parameters[1];
		public IExpression ElseExpr => Parameters[2];

		public TernaryExpression(IExpression condition, IExpression thenExpr, IExpression elseExpr) : base(condition, thenExpr, elseExpr)
		{
			
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			if (Condition.Evaluate(scope, environment).Equals(DoubleValue.One))
			{
				return ThenExpr == null ? Condition.Evaluate(scope, environment) :
					ThenExpr.Evaluate(scope, environment);
			}
			else if (ElseExpr != null)
			{
				return ElseExpr.Evaluate(scope, environment);
			}

			return DoubleValue.Zero;
		}
	}
}