using System;
using System.Collections.Generic;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	/// <summary>
	///		Represents a readonly query structure
	/// </summary>
	public class QueryStruct : IMoStruct
	{
		protected readonly IDictionary<string, Func<MoParams, object>> Functions =
			new Dictionary<string, Func<MoParams, object>>(StringComparer.OrdinalIgnoreCase);

		/// <inheritdoc />
		public object Value => this;

		public QueryStruct() { }

		public QueryStruct(IEnumerable<KeyValuePair<string, Func<MoParams, object>>> parameters)
		{
			Functions = new Dictionary<string, Func<MoParams, object>>(parameters);
		}

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			throw new NotSupportedException("Cannot set a value in a query struct.");
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (Functions.TryGetValue(key.Value, out var func))
			{
				return MoValue.FromObject(func(parameters));
			}
			
			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Functions.Clear();
		}
	}
}