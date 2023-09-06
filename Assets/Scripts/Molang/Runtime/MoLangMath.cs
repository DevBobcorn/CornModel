using System;
using System.Collections.Generic;
using CraftSharp.Molang.Attributes;
using CraftSharp.Molang.Runtime.Struct;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Runtime
{
	/// <summary>
	///		The default Math implementations for the MoLang runtime
	/// </summary>
	public static class MoLangMath
	{
		/// <summary>
		///		The Math library
		/// </summary>
		public static readonly QueryStruct Library = new QueryStruct(
			new Dictionary<string, Func<MoParams, object>>(StringComparer.OrdinalIgnoreCase)
			{
				{ "abs", param => Math.Abs(param.GetDouble(0)) },
				{ "acos", param => Math.Acos(param.GetDouble(0)) },
				{ "sin", param => Math.Sin(param.GetDouble(0) * (Math.PI / 180d)) },
				{ "asin", param => Math.Asin(param.GetDouble(0)) },
				{ "atan", param => Math.Atan(param.GetDouble(0)) },
				{ "atan2", param => Math.Atan2(param.GetDouble(0), param.GetDouble(1)) },
				{ "ceil", param => Math.Ceiling(param.GetDouble(0)) },
				{
					"clamp", param => Math.Min(param.GetDouble(1), Math.Max(param.GetDouble(0), param.GetDouble(2)))
				},
				{ "cos", param => Math.Cos(param.GetDouble(0) * (Math.PI / 180d)) },
				{ "die_roll", param => DieRoll(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "die_roll_integer", param => DieRollInt(param.GetInt(0), param.GetInt(1), param.GetInt(2)) },
				{ "exp", param => Math.Exp(param.GetDouble(0)) },
				{ "mod", param => param.GetDouble(0) % param.GetDouble(1) },
				{ "floor", param => Math.Floor(param.GetDouble(0)) },
				{ "hermite_blend", param => HermiteBlend(param.GetInt(0)) },
				{ "lerp", param => Lerp(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "lerp_rotate", param => LerpRotate(param.GetDouble(0), param.GetDouble(1), param.GetDouble(2)) },
				{ "ln", param => Math.Log(param.GetDouble(0)) },
				{ "max", param => Math.Max(param.GetDouble(0), param.GetDouble(1)) },
				{ "min", param => Math.Min(param.GetDouble(0), param.GetDouble(1)) },
				{ "pi", param => Math.PI },
				{ "pow", param => Math.Pow(param.GetDouble(0), param.GetDouble(1)) },
				{ "random", param => Random(param.GetDouble(0), param.GetDouble(1)) },
				{ "random_integer", param => RandomInt(param.GetInt(0), param.GetInt(1)) },
				{ "round", param => Math.Round(param.GetDouble(0)) },
				{ "sqrt", param => Math.Sqrt(param.GetDouble(0)) },
				{ "trunc", param => Math.Floor(param.GetDouble(0)) },
			});

		public static double Random(double low, double high)
		{
			return low + _random.NextDouble() * (high - low);
		}

		private static Random _random = new Random();

		public static int RandomInt(int low, int high)
		{
			return _random.Next(low, high);
		}

		public static double DieRoll(double num, double low, double high)
		{
			int i = 0;
			double total = 0;
			while (i++ < num) total += Random(low, high);

			return total;
		}

		public static int DieRollInt(int num, int low, int high)
		{
			int i = 0;
			int total = 0;
			while (i++ < num) total += RandomInt(low, high);

			return total;
		}

		public static int HermiteBlend(int value)
		{
			return (3 * value) ^ (2 - 2 * value) ^ 3;
		}

		public static double Lerp(double start, double end, double amount)
		{
			amount = Math.Max(0, Math.Min(1, amount));

			return start + (end - start) * amount;
		}

		public static double LerpRotate(double start, double end, double amount)
		{
			start = Radify(start);
			end = Radify(end);

			if (start > end)
			{
				(start, end) = (end, start);
			}

			if (end - start > 180)
			{
				return Radify(end + amount * (360 - (end - start)));
			}

			return start + amount * (end - start);
		}

		public static double Radify(double num)
		{
			return (((num + 180) % 360) + 180) % 360;
		}
	}
	
	/// <summary>
	///		The default Math implementations for the MoLang runtime
	/// </summary>
	public sealed class MoLangMathImpl
	{
		private static IMoStruct _instance;
		public static IMoStruct Library => _instance ??= new InteropStruct(new MoLangMathImpl());
		
		private MoLangMathImpl()
		{
			
		}
		
		[MoFunction("abs")] 
		public double Abs(double value) => Math.Abs(value);
		
		[MoFunction("sin")] 
		public double Sin(double value) => Math.Sin(value * (Math.PI / 180d));
		
		[MoFunction("asin")] 
		public double Asin(double value) => Math.Asin(value);
		
		[MoFunction("cos")] 
		public double Cos(double value) => Math.Cos(value * (Math.PI / 180d));
		
		[MoFunction("acos")] 
		public double Acos(double value) => Math.Acos(value);
		
		[MoFunction("atan")] 
		public double Atan(double value) => Math.Atan(value);
		
		[MoFunction("atan2")] 
		public double Atan2(double y, double x) => Math.Atan2(y, x);
		
		[MoFunction("ceil")]
		public double Ceiling(double value) => Math.Ceiling(value);

		[MoFunction("clamp")]
		public double Clamp(double value, double min, double max) => Math.Clamp(value, min, max);
		
		[MoFunction("die_roll")]
		public double DieRoll(double num, double low, double high)
		{
			int i = 0;
			double total = 0;
			while (i++ < num) total += Random(low, high);

			return total;
		}
		
		[MoFunction("die_roll_integer")]
		public int DieRollInt(int num, int low, int high)
		{
			int i = 0;
			int total = 0;
			while (i++ < num) total += RandomInt(low, high);

			return total;
		}

		[MoFunction("exp")]
		public double Exp(double value) => Math.Exp(value);
		
		[MoFunction("mod")]
		public double Modulus(double x, double y) => x % y;

		[MoFunction("floor")]
		public double Floor(double value) => Math.Floor(value);

		[MoFunction("hermite_blend")]
		public int HermiteBlend(int value) => (3 * value) ^ (2 - 2 * value) ^ 3;

		[MoFunction("lerp")]
		public double Lerp(double start, double end, double amount)
		{
			amount = Math.Max(0, Math.Min(1, amount));

			return start + (end - start) * amount;
		}
		
		[MoFunction("lerp_rotate")]
		public double LerpRotate(double start, double end, double amount)
		{
			start = Radify(start);
			end = Radify(end);

			if (start > end)
			{
				(start, end) = (end, start);
			}

			if (end - start > 180)
			{
				return Radify(end + amount * (360 - (end - start)));
			}

			return start + amount * (end - start);
		}

		[MoFunction("ln")]
		public double Log(double value) => Math.Log(value);

		[MoFunction("max")]
		public double Max(double value1, double value2) => Math.Max(value1, value2);
		
		[MoFunction("min")]
		public double Min(double value1, double value2) => Math.Min(value1, value2);

		[MoFunction("pi")]
		public double PiFunc() => Math.PI;
		
		[MoProperty("pi")]
		public double PI => Math.PI;

		[MoFunction("pow")]
		public double Pow(double x, double y) => Math.Pow(x, y);
		
		[MoFunction("random")]
		public double Random(double low, double high)
		{
			return low + _random.NextDouble() * (high - low);
		}
		
		[MoFunction("random_integer")]
		public int RandomInt(int low, int high)
		{
			return _random.Next(low, high);
		}

		[MoFunction("round")]
		public double Round(double value) => Math.Round(value);
		
		[MoFunction("sqrt")]
		public double Sqrt(double value) => Math.Sqrt(value);
		
		[MoFunction("trunc")]
		public double Truncate(double value) => Math.Floor(value);
		
		public double Radify(double num)
		{
			return (((num + 180) % 360) + 180) % 360;
		}

		private Random _random = new Random();
	}
}