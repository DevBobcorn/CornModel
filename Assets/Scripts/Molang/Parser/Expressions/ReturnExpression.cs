using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class ReturnExpression : Expression
	{
		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue eval = Parameters[0].Evaluate(scope, environment);
			scope.ReturnValue = eval;

			return eval;
		}

		/// <inheritdoc />
		public ReturnExpression(IExpression value) : base(value)
		{
		}
	}
}