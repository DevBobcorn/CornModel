using System;
using System.Collections.Generic;
using CraftSharp.Molang.Runtime.Exceptions;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	/// <summary>
	///		Represents a dynamic variable
	/// </summary>
	public class VariableStruct : IMoStruct
	{
		public IDictionary<string, IMoValue> Map { get; protected set; }

		/// <inheritdoc />
		public object Value => Map;

		public VariableStruct()
		{
			Map = new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase);
		}

		public VariableStruct(IEnumerable<KeyValuePair<string, IMoValue>> values)
		{
			if (values != null)
			{
				Map = new Dictionary<string, IMoValue>(values, StringComparer.OrdinalIgnoreCase);
			}
			else
			{
				Map = new Dictionary<string, IMoValue>(StringComparer.OrdinalIgnoreCase);
			}
		}

		protected virtual IMoStruct CreateNew()
		{
			return new VariableStruct();
		}

		/// <inheritdoc />
		public virtual void Set(MoPath key, IMoValue value)
		{
			if (!key.HasChildren)
			{
				Map[key.Value] = value;

				return;
			}

			string main = key.Value;

			if (!string.IsNullOrWhiteSpace(main))
			{
				if (!Map.TryGetValue(main, out var container))
				{
					if (!key.HasChildren)
					{
						throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
					}

					Map.TryAdd(main, container = CreateNew());
				}

				if (container is IMoStruct moStruct)
				{
					moStruct.Set(key.Next, value);
				}
				else
				{
					throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
				}
			}
		}

		/// <inheritdoc />
		public virtual IMoValue Get(MoPath key, MoParams parameters)
		{
			if (key.HasChildren)
			{
				string main = key.Value;

				if (!string.IsNullOrWhiteSpace(main))
				{
					IMoValue value = null;

					if (!Map.TryGetValue(main, out value))
					{
						return DoubleValue.Zero;
					}

					if (value is IMoStruct moStruct)
					{
						return moStruct.Get(key.Next, parameters);
					}

					return value;
				}
			}

			if (Map.TryGetValue(key.Value, out var v))
				return v;

			return DoubleValue.Zero;
		}

		/// <inheritdoc />
		public void Clear()
		{
			Map.Clear();
		}
	}
}