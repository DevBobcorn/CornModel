using System;
using System.Collections.Generic;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	public class ContextStruct : VariableStruct
	{
		internal IDictionary<string, IMoValue> Container
		{
			get
			{
				return Map;
			}
			set
			{
				Map = value;
			}
		}

		public ContextStruct() : base() { }

		public ContextStruct(IEnumerable<KeyValuePair<string, IMoValue>> values) : base(values) { }

		/// <inheritdoc />
		public override void Set(MoPath key, IMoValue value)
		{
			throw new NotSupportedException("Read-only context");
		}
	}
}