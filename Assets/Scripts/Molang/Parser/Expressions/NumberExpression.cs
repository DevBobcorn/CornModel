using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class NumberExpression : Expression
	{
		private readonly IMoValue _value;

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return _value;
		}

		/// <inheritdoc />
		public NumberExpression(double value) : base()
		{
			_value = new DoubleValue(value);
		}
		
		public NumberExpression(IMoValue value) : base()
		{
			_value = value;
		}
	}
}