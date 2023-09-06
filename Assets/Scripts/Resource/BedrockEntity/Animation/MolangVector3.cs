#nullable enable
using Unity.Mathematics;

using CraftSharp.Molang.Runtime;
using CraftSharp.Molang.Parser;

namespace CraftSharp.Resource
{
    public class MolangVector3
    {
        public static readonly MolangVector3 ZERO = new(null, 0F);

        public readonly IExpression? x;
        public readonly float xValue;
        public readonly IExpression? y;
        public readonly float yValue;
        public readonly IExpression? z;
        public readonly float zValue;

        // Json data could be [ 1, 1, 1 ] or [ 1.0 ] or just 1.0 (without brackets)
        // which will all be converted to a (1F, 1F, 1F)
        // See https://learn.microsoft.com/en-us/minecraft/creator/reference/content/animationsreference/examples/animationgettingstarted
        public static MolangVector3 FromJson(Json.JSONData data)
        {
            if (data.Type == Json.JSONData.DataType.String) // Single value
            {
                var (exp, val) = ReadMolangExpressionOrValue(data);
                return new MolangVector3(exp, val, exp, val, exp, val);
            }
            else if (data.Type == Json.JSONData.DataType.Array) // 1-value array or 3-value array
            {
                if (data.DataArray.Count == 1)
                {
                    var (exp, val) = ReadMolangExpressionOrValue(data.DataArray[0]);
                    return new MolangVector3(exp, val, exp, val, exp, val);
                }
                else if (data.DataArray.Count == 3)
                {
                    var (exp0, val0) = ReadMolangExpressionOrValue(data.DataArray[0]);
                    var (exp1, val1) = ReadMolangExpressionOrValue(data.DataArray[1]);
                    var (exp2, val2) = ReadMolangExpressionOrValue(data.DataArray[2]);
                    return new MolangVector3(exp0, val0, exp1, val1, exp2, val2);
                }
                else
                {
                    throw new System.Exception($"Invalid vector length in keyframe data: {data.DataArray.Count}");
                }
            }
            else
            {
                throw new System.Exception($"Invalid data type in keyframe data: {data.Type}");
            }
        }

        private static (IExpression? exp, float val) ReadMolangExpressionOrValue(Json.JSONData data)
        {
            if (float.TryParse(data.StringValue, out float val)) // Literal number
            {
                return (null, val);
            }
            else // MoLang expression
            {
                return (MoLangParser.Parse(data.StringValue), 0F);
            }
        }

        public MolangVector3(IExpression? _x, float xv, IExpression? _y, float yv, IExpression? _z, float zv)
        {
            x = _x;
            xValue = xv;
            y = _y;
            yValue = yv;
            z = _z;
            zValue = zv;
        }

        public MolangVector3(IExpression? _x, float xv)
        {
            x = _x;
            xValue = xv;
            y = _x;
            yValue = xv;
            z = _x;
            zValue = xv;
        }

        public float3 Evaluate(MoScope scope, MoLangEnvironment env)
        {
            return new(
                    x is null ? xValue : x.Evaluate(scope, env).AsFloat(),
                    y is null ? yValue : y.Evaluate(scope, env).AsFloat(),
                    z is null ? zValue : z.Evaluate(scope, env).AsFloat()
            );
        }

        public override string ToString()
        {
            return $"({(x is null ? xValue : x)}, {(y is null ? yValue : y)}, {(z is null ? zValue : z)})";
        }
    }
}