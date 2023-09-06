using System;
using System.Collections.Generic;
using System.Linq;
using CraftSharp.Molang.Runtime.Exceptions;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	/// <summary>
	///		Represents an array of values
	/// </summary>
	public class ArrayStruct : IMoStruct
	{
		private IMoValue[] _array;

		/// <summary>
		///		Initializes a new instance of the ArrayStruct class with no values.
		/// </summary>
		public ArrayStruct()
		{
			_array = new IMoValue[0];
		}

		/// <summary>
		///		Initializes a new instance of the ArrayStruct class.
		/// </summary>
		/// <param name="values">The values this ArrayStruct contains</param>
		public ArrayStruct(IEnumerable<IMoValue> values)
		{
			_array = values.ToArray();
		}

		/// <summary>
		///		Indexes the ArrayStruct
		/// </summary>
		/// <param name="index">The index to get/set the value of</param>
		public IMoValue this[int index]
		{
			get
			{
				if (index >= _array.Length)
					return DoubleValue.Zero;
				
				return _array[index % _array.Length];
			}
			set
			{
				if (_array.Length == 0)
					return;
				
				_array[index % _array.Length] = value;
			}
		}

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			if (int.TryParse(key.Value, out int index))
			{
				this[index] = value;
			}
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (int.TryParse(key.Value, out int index))
			{
				return this[index];
			}

			throw new MoLangRuntimeException($"Invalid path for array access: {key.Path.ToString()}");
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotSupportedException("Cannot clear an ArrayStruct");
		}

		/// <inheritdoc />
		public object Value => _array;
	}

	public class VariableArrayStruct : VariableStruct
	{
		/// <inheritdoc />
		protected override IMoStruct CreateNew()
		{
			return new ArrayStruct();
		}
	}
}