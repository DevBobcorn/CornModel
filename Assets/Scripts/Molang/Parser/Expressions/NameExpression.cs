using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Parser.Expressions
{
	public class NameExpression : Expression
	{
		public MoPath Name { get; set; }

		/// <inheritdoc />
		public override IMoValue Evaluate(MoScope scope, MoLangEnvironment environment)
		{
			return environment.GetValue(Name);
		}

		/// <inheritdoc />
		public override void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
		{
			environment.SetValue(Name, value);
		}

		/// <inheritdoc />
		public NameExpression(string value)
		{
			Name = new MoPath(value);
		}

		public NameExpression(MoPath path)
		{
			Name = path;
		}
	}
}