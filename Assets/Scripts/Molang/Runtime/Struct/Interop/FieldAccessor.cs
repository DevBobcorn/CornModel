using System.Reflection;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Runtime.Struct.Interop
{
	public class FieldAccessor : ValueAccessor
	{
		private readonly FieldInfo _fieldInfo;

		public FieldAccessor(FieldInfo fieldInfo)
		{
			_fieldInfo = fieldInfo;
		}

		public override bool CanRead => true;
		public override bool CanWrite => !_fieldInfo.IsInitOnly;
		
		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _fieldInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
		}
		
		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			_fieldInfo.SetValue(instance, value);
			InvokeChanged();
		}
	}
}