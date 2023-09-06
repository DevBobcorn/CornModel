using System.Linq;
using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Struct;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class ForEachExpression : Expression
	{
		public IExpression Variable => Parameters[0];
		public IExpression Array => Parameters[1];
		public IExpression Body => Parameters[2];

		public ForEachExpression(IExpression variable, IExpression array, IExpression body) : base(variable, array, body)
		{
			
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			IMoValue array = Array.Evaluate(scope, environment);

			if (array is VariableStruct vs)
			{
				MoScope subScope = new MoScope(scope.Runtime);

				foreach (IMoValue value in vs.Map.Values)
				{
					subScope.IsContinue = false;
					subScope.IsBreak = false;

					Variable.Assign(
						subScope, environment, value is VariableStruct vss ? vss.Map.FirstOrDefault().Value : value);

					Body.Evaluate(subScope, environment);

					if (subScope.ReturnValue != null)
					{
						return subScope.ReturnValue;
					}
					else if (subScope.IsBreak)
					{
						break;
					}
				}
			}

			return DoubleValue.Zero;
		}
	}
}