using System;
using System.Linq;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Runtime;

namespace CraftSharp.Molang.Parser.Visitors
{
    /// <summary>
    ///     Optimizes expressions by pre-calculating constant maths
    /// </summary>
    public class MathOptimizationVisitor : ExpressionVisitor
    {
        private static MoScope _scope = new MoScope(new MoLangRuntime());
        private static MoLangEnvironment _environment = new MoLangEnvironment();

        public override IExpression OnVisit(ExpressionTraverser traverser, IExpression expression)
        {
            return Visit(expression);
        }

        private static IExpression Visit(IExpression expression)
        {
            if (expression is FuncCallExpression nameExpression &&
                nameExpression.Name.Value.Equals("math", StringComparison.InvariantCultureIgnoreCase))
            {
                return TryOptimizeMathFunction(nameExpression);
            }
            
            if (expression is BinaryOpExpression binaryOp)
            {
                return TryOptimize(binaryOp);
            }

            return expression;
        }
        
        private static IExpression TryOptimizeMathFunction(FuncCallExpression expression)
        {
            for (int i = 0; i < expression.Parameters.Length; i++)
            {
                expression.Parameters[i] = Visit(expression.Parameters[i]);
            }
            
            if (expression.Parameters.All(x => x is NumberExpression))
            {
                var eval = expression.Evaluate(_scope, _environment);
                return new NumberExpression(eval);
            }
            
            return expression;
        }

        private static IExpression TryOptimize(BinaryOpExpression expression)
        {
            if (expression.Left is BinaryOpExpression l)
                expression.Left = TryOptimize(l);
            
            if (expression.Right is BinaryOpExpression r)
                expression.Right = TryOptimize(r);
            
            if (expression.Left is NumberExpression && expression.Right is NumberExpression)
            {
                //Can be pre-calculated!
                var eval = expression.Evaluate(_scope, _environment);
                return new NumberExpression(eval);
            }

            return expression;
        }
    }
}