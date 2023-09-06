using System;
using System.Collections.Generic;

namespace CraftSharp.Molang.Parser.Visitors
{
	public class FindingVisitor : ExpressionVisitor
	{
		private Predicate<IExpression> _predicate;
		public List<IExpression> FoundExpressions = new List<IExpression>();

		public FindingVisitor(Predicate<IExpression> predicate)
		{
			_predicate = predicate;
		}

		/// <inheritdoc />
		public override IExpression OnVisit(ExpressionTraverser traverser, IExpression expression)
		{
			if (_predicate(expression))
			{
				FoundExpressions.Add(expression);
			}

			return expression;
		}
	}
}