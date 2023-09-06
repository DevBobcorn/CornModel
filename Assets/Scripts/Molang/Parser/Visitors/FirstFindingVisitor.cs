using System;

namespace CraftSharp.Molang.Parser.Visitors
{
	public class FirstFindingVisitor : ExpressionVisitor
	{
		private Predicate<IExpression> _predicate;
		public IExpression Found = null;

		public FirstFindingVisitor(Predicate<IExpression> predicate)
		{
			_predicate = predicate;
		}

		/// <inheritdoc />
		public override IExpression OnVisit(ExpressionTraverser traverser, IExpression expression)
		{
			if (_predicate(expression))
			{
				Found = expression;

				traverser.Stop();
			}

			return expression;
		}
	}
}