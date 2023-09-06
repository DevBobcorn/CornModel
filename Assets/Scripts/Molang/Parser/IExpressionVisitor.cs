namespace CraftSharp.Molang.Parser
{
	/// <summary>
	///		The base interface for expression visitors.
	/// </summary>
	/// <remarks>
	///		Expression visitors are mainly used to optimize arrays of <see cref="IExpression"/>
	///		One such example is the <see cref="Visitors.MathOptimizationVisitor"/>
	/// </remarks>
	public interface IExpressionVisitor
	{
		/// <summary>
		///		Invoked before any ExpressionVisitor has traversed expression array
		/// </summary>
		/// <param name="expressions">The expressions to be traversed</param>
		void BeforeTraverse(IExpression[] expressions);

		/// <summary>
		///		Invoked once for every <see cref="IExpression"/>
		/// </summary>
		/// <param name="traverser">The <see cref="ExpressionTraverser"/> that invoked the visitor</param>
		/// <param name="expression">The expressions to visit</param>
		/// <returns>The visited expression</returns>
		IExpression OnVisit(ExpressionTraverser traverser, IExpression expression);

		/// <summary>
		///		Invoked when all ExpressionVisitors have visited an expression
		/// </summary>
		/// <param name="expression">The expressions that has been visited</param>
		void OnLeave(IExpression expression);

		/// <summary>
		///		Invoked after all of the ExpressionVisitors have traversed an expression array
		/// </summary>
		/// <param name="expressions">The visited expression array</param>
		void AfterTraverse(IExpression[] expressions);
	}
}