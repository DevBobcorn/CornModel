using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CraftSharp.Molang.Runtime.Exceptions;

namespace CraftSharp.Molang.Parser
{
	/// <summary>
	///		Traverses arrays of <see cref="IExpression"/> with <see cref="IExpressionVisitor"/>'s
	/// </summary>
	public class ExpressionTraverser
	{
		/// <summary>
		///		The list of <see cref="IExpressionVisitor"/> that will visit the traversed expressions
		/// </summary>
		public readonly List<IExpressionVisitor> Visitors = new List<IExpressionVisitor>();

		private bool _stop = false;

		/// <summary>
		///		Traverse an array of expressions.
		/// </summary>
		/// <param name="expressions">The array of expressions to visit</param>
		/// <returns>The traversed array of expressions</returns>
		public IExpression[] Traverse(IExpression[] expressions)
		{
			TraverseArray(expressions);

			return expressions.Where(x => x != null).ToArray();
		}

		private void TraverseArray(IExpression[] expressions)
		{
			foreach (IExpressionVisitor visitor in Visitors)
			{
				visitor.BeforeTraverse(expressions);
			}

			for (var index = 0; index < expressions.Length; index++)
			{
				IExpression expression = expressions[index];

				if (expression == null)
					throw new MoLangRuntimeException("Expression was null", null);

				expressions[index] = TraverseExpr(expression, null);

				if (_stop)
				{
					break;
				}
			}

			foreach (IExpressionVisitor visitor in Visitors)
			{
				visitor.AfterTraverse(expressions);
			}
		}

		private IExpression TraverseExpr(IExpression expression, IExpression parent)
		{
			expression = Visit(expression);
			expression.Meta.Parent = parent;

			var parameters = expression.Parameters;
			for (var index = 0; index < parameters.Length; index++)
			{
				parameters[index] = TraverseExpr(parameters[index], expression);
				
				if (_stop)
				{
					break;
				}
			}

			expression.Parameters = parameters;

			OnLeave(expression);

			return expression;
		}

		private IExpression Visit(IExpression expression)
		{
			foreach (var visitor in Visitors)
			{
				expression = visitor.OnVisit(this, expression);
			}

			return expression;
		}

		private void OnLeave(IExpression expression)
		{
			foreach (var visitor in Visitors)
			{
				visitor.OnLeave(expression);
			}
		}

		public void Stop()
		{
			_stop = true;
		}

		private static ConcurrentDictionary<Type, PropertyInfo[]> _cachedProperties =
			new ConcurrentDictionary<Type, PropertyInfo[]>();

		private static PropertyInfo[] GetAllProperties(Type type)
		{
			return _cachedProperties.GetOrAdd(
				type, t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray());
		}

		private object GetFieldValue(PropertyInfo field, object obj)
		{
			return field.GetValue(obj);
		}

		private void SetFieldValue(PropertyInfo field, object obj, object value)
		{
			field.SetValue(obj, value);
		}

		[Flags]
		public enum VisitationResult
		{
			None,

			RemoveCurrent = 0x01,
			StopTraversal = 0x02,
			DontTraverseChildren = 0x04,
			DontTraverseCurrent = 0x08,
			DontTraverseCurrentAndChildren = DontTraverseCurrent | DontTraverseChildren
		}
	}
}