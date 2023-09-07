#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using CraftSharp.Molang.Parser;
using CraftSharp.Molang.Parser.Expressions;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Resource
{
    public static class MolangExpressionExtension
    {
        private const int MAX_EXPRESSION_COUNT = 10000;

        public static HashSet<MoPath> GetReferencedVariables(this IExpression exp)
        {
            Queue<IExpression> expQueue = new();
            HashSet<MoPath> variables = new();

            int count = 0;

            for (int i = 0; i < exp.Parameters.Length; i++)
            {
                expQueue.Enqueue(exp.Parameters[i]);
            }

            while (expQueue.Count > 0)
            {
                var cur = expQueue.Dequeue();

                if (cur is NameExpression nameExp)
                {
                    variables.Add(nameExp.Name);
                }

                for (int i = 0; i < cur.Parameters.Length; i++)
                {
                    expQueue.Enqueue(cur.Parameters[i]);
                }
                
                count++;

                if (count > MAX_EXPRESSION_COUNT)
                {
                    Debug.LogWarning("Too many sub expressions in MoLang!");
                    break;
                }
            }

            return variables;
        }
    }
}