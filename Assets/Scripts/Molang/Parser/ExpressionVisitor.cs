namespace CraftSharp.Molang.Parser
{
    /// <summary>
    ///     An abstract class that can be used as a base for any ExpressionVisitor
    /// </summary>
    public abstract class ExpressionVisitor : IExpressionVisitor
    {
        /// <inheritdoc />
        public virtual void BeforeTraverse(IExpression[] expressions) { }

        /// <inheritdoc />
        public abstract IExpression OnVisit(ExpressionTraverser traverser, IExpression expression);

        /// <inheritdoc />
        public virtual void OnLeave(IExpression expression) { }

        /// <inheritdoc />
        public virtual void AfterTraverse(IExpression[] expressions) { }
    }
}