using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Struct;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class ArrayAccessExpression : Expression
	{
		public IExpression Array => Parameters[0];
		public IExpression Index => Parameters[1];

		public ArrayAccessExpression(IExpression array, IExpression index) : base(array, index)
		{
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			var index = (int) Index.Evaluate(scope, environment).AsDouble();
			MoPath path;
			if (Array is NameExpression nameExpression)
			{
				var p = nameExpression.Name;
				path = p;
			}
			else
			{
				var eval = Array.Evaluate(scope, environment);
				path = new MoPath($"{eval.AsString()}");
			}

			var array = environment.GetValue(path);
			if (array is ArrayStruct asArray)
				return asArray[index];
			
			return environment.GetValue(path);
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			var index = (int) Index.Evaluate(scope, environment).AsDouble();
			
			MoPath path;
			if (Array is NameExpression nameExpression)
			{
				var p = nameExpression.Name;
				path = p;
			}
			else
			{
				var eval = Array.Evaluate(scope, environment);
				path = new MoPath($"{eval.AsString()}");
			}
			
			var array = environment.GetValue(path);
			if (array is ArrayStruct asArray)
			{
				asArray[index] = value;
			}
		}
	}
}