using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using CraftSharp.Molang.Attributes;
using CraftSharp.Molang.Runtime.Exceptions;
using CraftSharp.Molang.Runtime.Struct.Interop;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	/// <summary>
	///		Provides an interoperability layer around an object 
	/// </summary>
	/// <remarks>
	///		Allows MoLang to access fields, properties & functions in a .NET object
	///
	///		Any fields or properties that you want to allow access to requires the <see cref="MoPropertyAttribute"/> to be applied.
	///
	///		Any functions that should be accessible by MoLang require the <see cref="MoFunctionAttribute"/> to be applied
	/// </remarks>
	public class InteropStruct : IMoStruct
	{
		private object _instance;
		private readonly PropertyCache _propertyCache;

		private static readonly ConcurrentDictionary<Type, PropertyCache> PropertyCaches =
			new ConcurrentDictionary<Type, PropertyCache>();

		public InteropStruct(object instance)
		{
			_instance = instance;

			var type = instance.GetType();

			_propertyCache = PropertyCaches.GetOrAdd(type, t => new PropertyCache(t));
		}

		/// <inheritdoc />
		public object Value => _instance;

		/// <inheritdoc />
		public void Set(MoPath key, IMoValue value)
		{
			if (!key.HasChildren)
			{
				if (_propertyCache.Properties.TryGetValue(key.Value, out var accessor))
				{
					if (!accessor.CanWrite)
						throw new MoLangRuntimeException("Cannot write to ReadOnly property!", null);

					accessor.Set(_instance, value);

					return;
				}

				throw new MoLangRuntimeException($"Variable was not a struct: {key}", null);
			}

			string main = key.Value;

			if (!string.IsNullOrWhiteSpace(main))
			{
				if (!_propertyCache.Properties.TryGetValue(main, out var accessor))
				{
					throw new MoLangRuntimeException($"Could not access property: {key}", null);
				}

				var container = accessor.Get(_instance);

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

		private bool ExecuteGet(MoPath property, string main, MoParams parameters, out IMoValue returnValue)
		{
			if (_propertyCache.Properties.TryGetValue(main, out var accessor))
			{
				if (!accessor.CanRead)
					throw new MoLangRuntimeException($"Cannot read from property '{property.ToString()}'", null);

				returnValue = accessor.Get(_instance);
				return true;
			}

			if (_propertyCache.Functions.TryGetValue(main, out var f))
			{
				returnValue = f.Invoke(_instance, parameters);
				return true;
			}

			returnValue = DoubleValue.Zero;
			return false;
		}

		/// <inheritdoc />
		public IMoValue Get(MoPath key, MoParams parameters)
		{
			if (key.HasChildren)
			{
				string main = key.Value;

				if (!string.IsNullOrWhiteSpace(main))
				{
					ExecuteGet(key, main, parameters, out var value);

					if (value is IMoStruct moStruct)
					{
						return moStruct.Get(key.Next, parameters);
					}

					return value;
				}
			}

			if (!ExecuteGet(key, key.Value, parameters, out var returnValue))
				Debug.WriteLine($"({_instance.ToString()}) Unknown query: {key}");

			return returnValue;
		}

		/// <inheritdoc />
		public void Clear()
		{
			throw new NotSupportedException("Cannot clear an InteropStruct.");
		}
	}
}