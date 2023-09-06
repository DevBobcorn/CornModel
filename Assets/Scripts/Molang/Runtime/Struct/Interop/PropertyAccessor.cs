using System.Reflection;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Runtime.Struct.Interop
{
	public class PropertyAccessor : ValueAccessor
	{
		private readonly PropertyInfo _propertyInfo;

		public PropertyAccessor(PropertyInfo propertyInfo)
		{
			_propertyInfo = propertyInfo;
		}

		public override bool CanRead => _propertyInfo.CanRead;
		public override bool CanWrite => _propertyInfo.CanWrite;
		
		/// <inheritdoc />
		public override IMoValue Get(object instance)
		{
			var value = _propertyInfo.GetValue(instance);

			return value is IMoValue moValue ? moValue : MoValue.FromObject(value);
		}

		/// <inheritdoc />
		public override void Set(object instance, IMoValue value)
		{
			var propType = _propertyInfo.PropertyType;

			if (propType == typeof(double))
			{
				_propertyInfo.SetValue(instance, value.AsDouble());
				return;
			}
			else if (propType == typeof(float))
			{
				_propertyInfo.SetValue(instance, value.AsFloat());
				return;
			}
			else if (propType == typeof(bool))
			{
				_propertyInfo.SetValue(instance, value.AsBool());
				return;
			}
			else if (propType == typeof(string))
			{
				_propertyInfo.SetValue(instance, value.AsString());
				return;
			}
			
			_propertyInfo.SetValue(instance, value);
			InvokeChanged();
		}
	}
}