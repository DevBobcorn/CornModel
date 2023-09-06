using System;

namespace CraftSharp.Molang.Runtime.Value
{
	/// <summary>
	///		Represents a double-precision floating-point number
	/// </summary>
	public class DoubleValue : IMoValue
	{
		private readonly double _value;

		/// <inheritdoc />
		object IMoValue.Value => _value;

		/// <inheritdoc />
		public double Value => _value;
		
		/// <inheritdoc />
		public bool Equals(IMoValue b)
		{
			if (_value == b.AsDouble())
				return true;

			return false;
		}

		public DoubleValue(object value)
		{
			if (value is bool boolean)
			{
				_value = boolean ? 1.0 : 0.0;
			}
			else if (value is double dbl)
			{
				_value = dbl;
			}
			else if (value is float flt)
			{
				_value = flt;
			}
			else if (value is int integer)
			{
				_value = integer;
			}
			else
			{
				throw new NotSupportedException($"Cannot convert {value.GetType().FullName} to double");
			}
		}

		public DoubleValue(bool value)
		{
			_value = value ? 1d : 0d;
		}

		public DoubleValue(double value)
		{
			_value = value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;

			return (obj is DoubleValue dv && dv._value == _value);
			// ...the rest of the equality implementation
		}

		public bool Equals(DoubleValue other)
		{
			return _value.Equals(other._value);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		/// <inheritdoc />
		public double AsDouble()
		{
			return _value;
		}

		/// <inheritdoc />
		public float AsFloat()
		{
			return (float)_value;
		}

		public static DoubleValue Zero { get; } = new DoubleValue(0d);
		public static DoubleValue One { get; } = new DoubleValue(1d);
	}
}