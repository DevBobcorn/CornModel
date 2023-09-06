namespace CraftSharp.Molang.Parser.Expressions
{
	public abstract class BinaryOpExpression : Expression
	{
		public IExpression Left
		{
			get { return Parameters[0]; }
			set
			{
				Parameters[0] = value;
			}
		}

		public IExpression Right
		{
			get { return Parameters[1]; }
			set { Parameters[1] = value; }
		}

		protected BinaryOpExpression(IExpression l, IExpression r) : base(l,r)
		{
		}

		public abstract string GetSigil();
	}
}