using System;
using System.Collections;
using System.Collections.Generic;
using CraftSharp.Molang.Runtime.Struct;

namespace CraftSharp.Molang.Runtime.Value
{
	/// <summary>
	///		Resembles a MoLang compatible value. 
	/// </summary>
	public interface IMoValue : IEquatable<IMoValue>
	{
		object Value { get; }

		virtual string AsString() => Value.ToString();

		virtual double AsDouble() => Value is double db ? db : 0d;

		virtual float AsFloat() => Value is float flt ? flt : (float)AsDouble();

		virtual bool AsBool() => Value is bool b ? b : AsDouble() > 0;
	}

	public static class MoValue
	{
		public static IMoValue FromObject(object value)
		{
			if (value is IMoValue moValue)
			{
				return moValue;
			}

			if (value is string str)
			{
				return new StringValue(str);
			}

			if (value is IEnumerable enumerable)
			{
				List<IMoValue> values = new List<IMoValue>();

				foreach (var enumObject in enumerable)
				{
					values.Add(FromObject(enumObject));
				}

				return new ArrayStruct(values);
			}

			return new DoubleValue(value);
		}
	}
}