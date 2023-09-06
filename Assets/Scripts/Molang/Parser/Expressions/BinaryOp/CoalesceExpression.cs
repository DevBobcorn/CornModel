using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser.Expressions.BinaryOp
{
	public class CoalesceExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public CoalesceExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue evalLeft = Left.Evaluate(scope, environment);
			
			//IMoValue value = environment.GetValue(new MoPath(evalLeft.AsString()));

			if (evalLeft == null || !evalLeft.AsBool())
			{
				return Right.Evaluate(scope, environment);
			}
			else
			{
				return evalLeft;
			}
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "??";
		}
	}
}