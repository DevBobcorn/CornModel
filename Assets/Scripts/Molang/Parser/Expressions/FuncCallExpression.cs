using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class FuncCallExpression : Expression
	{
		public MoPath Name { get; set; }
		public FuncCallExpression(MoPath name, IExpression[] args) : base(args)
		{
			Name = name;
		}

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			//List<IExpression> p = Args.ToList();
			MoPath name = Name;/* Name is NameExpression expression ? expression.Name :
				new MoPath(Name.Evaluate(scope, environment).ToString());*/

			IMoValue[] arguments = new IMoValue[Parameters.Length];

			for (int i = 0; i < arguments.Length; i++)
			{
				arguments[i] = Parameters[i].Evaluate(scope, environment);
			}

			return environment.GetValue(name, new MoParams(arguments));
		}
	}
}