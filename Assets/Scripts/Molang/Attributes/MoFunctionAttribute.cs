using System;

namespace CraftSharp.Molang.Attributes
{
	/// <summary>
	///		Identifies a method as a MoLang Accessible function 
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class MoFunctionAttribute : Attribute
	{
		/// <summary>
		///		The names this method is exposed via
		/// </summary>
		public string[] Name { get; }

		/// <summary>
		///		Identifies a method as a MoLang Accessible function 
		/// </summary>
		/// <param name="functionNames">The names this function is callable by in a MoLang expression</param>
		public MoFunctionAttribute(params string[] functionNames)
		{
			Name = functionNames;
		}
	}
}