using System;
using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Exceptions;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions.BinaryOp
{
	public class PlusExpression : BinaryOpExpression
	{
		/// <inheritdoc />
		public PlusExpression(IExpression l, IExpression r) : base(l, r) { }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			try
			{
				return new DoubleValue(
					Left.Evaluate(scope, environment).AsDouble() + Right.Evaluate(scope, environment).AsDouble());
			}
			catch (Exception ex)
			{
				throw new MoLangRuntimeException(this, "An unexpected error occured.", ex);
			}
		}

		/// <inheritdoc />
		public override string GetSigil()
		{
			return "+";
		}
	}
}