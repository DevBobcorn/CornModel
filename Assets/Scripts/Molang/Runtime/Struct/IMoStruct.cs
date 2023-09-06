using System;
using CraftSharp.Molang.Runtime.Value;
using CraftSharp.Molang.Utils;

namespace CraftSharp.Molang.Runtime.Struct
{
	/// <summary>
	///		The interface used for all MoLang containers/structs
	/// </summary>
	public interface IMoStruct : IMoValue
	{
		/// <summary>
		///		Assign a value to a property
		/// </summary>
		/// <param name="key">The path of the property to modify</param>
		/// <param name="value">The value to set</param>
		void Set(MoPath key, IMoValue value);

		/// <summary>
		///		Get the value of a property
		/// </summary>
		/// <param name="key">The path of the property to get</param>
		/// <param name="parameters">The parameters used to retrieve the value</param>
		/// <returns>The value of the property retrieved</returns>
		IMoValue Get(MoPath key, MoParams parameters);

		/// <summary>
		///		Clears the struct
		/// </summary>
		void Clear();

		/// <inheritdoc />
		bool IEquatable<IMoValue>.Equals(IMoValue other)
		{
			return this.Equals((object)other);
		}
	}
}