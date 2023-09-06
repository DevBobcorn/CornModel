using System;

namespace CraftSharp.Molang.Attributes
{
	/// <summary>
	///		Identifies a class property/field as MoLang Accessible 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class MoPropertyAttribute : Attribute
	{
		/// <summary>
		///		The name of this property as used in a MoLang expression
		/// </summary>
		public string Name { get; }

		/// <summary>
		///		Identifies a class property/field as MoLang Accessible 
		/// </summary>
		/// <param name="name" example="frame_time">The name used for this property.</param>
		public MoPropertyAttribute(string name)
		{
			Name = name;
		}
	}
}