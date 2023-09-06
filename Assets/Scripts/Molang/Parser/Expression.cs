using System;
using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Parser
{
    /// <summary>
    ///     An abstract base class for all Expressions
    /// </summary>
    public abstract class Expression : IExpression
    {
        /// <summary>
        ///      The parameters used by this expression
        /// </summary>
        public IExpression[] Parameters { get; set; }

        /// <inheritdoc />
        public ExpressionMeta Meta { get; } = new ExpressionMeta();

        /// <summary>
        ///     Create a new instance of the class
        /// </summary>
        /// <param name="parameters">
        ///     The parameters used by this expression
        /// </param>
        protected Expression(params IExpression[] parameters)
        {
            Parameters = parameters;
        }
		
        /// <inheritdoc />
        public abstract IMoValue Evaluate(MoScope scope, MoLangEnvironment environment);

        /// <inheritdoc />
        public virtual void Assign(MoScope scope, MoLangEnvironment environment, IMoValue value)
        {
            throw new Exception("Cannot assign a value to " + this.GetType());
        }
    }
}