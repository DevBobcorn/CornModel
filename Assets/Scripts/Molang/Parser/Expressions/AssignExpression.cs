using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class AssignExpression : Expression
	{
		public IExpression Variable => Parameters[0];
		public IExpression Expression => Parameters[1];

		public AssignExpression(IExpression variable, IExpression expr) : base(variable, expr)
		{
			
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			Variable.Assign(scope, environment, Expression.Evaluate(scope, environment));

			return DoubleValue.Zero;
		}
	}
}