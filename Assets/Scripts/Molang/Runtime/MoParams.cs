using System.Linq;
using CraftSharp.Molang.Runtime.Exceptions;
using CraftSharp.Molang.Runtime.Struct;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Runtime
{
	/// <summary>
	///		Represents the parameters passed to a function by the MoLang expression
	/// </summary>
	public class MoParams
	{
		public static readonly MoParams Empty = new MoParams(new IMoValue[0]);

		private readonly IMoValue[] _parameters;

		public MoParams(params IMoValue[] param)
		{
			_parameters = param;
		}
		
		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as an <see cref="IMoValue"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public IMoValue Get(int index)
		{
			return _parameters[index];
		}

		/// <summary>
		///		Get the typed value of a parameter at specified <paramref name="index"/>
		/// </summary>
		/// <param name="index">The index of the </param>
		/// <typeparam name="T">The expected type</typeparam>
		/// <returns>
		///		The parameter passed at <paramref name="index"/>
		/// </returns>
		/// <exception cref="MoLangRuntimeException"></exception>
		public T Get<T>(int index)
		{
			IMoValue obj = _parameters[index];

			if (obj == null)
				throw new MoLangRuntimeException(
					$"MoParams: Expected parameter type of {typeof(T).Name} got null", null);

			if (obj?.GetType() == typeof(T))
			{
				return (T)obj;
			}
			else
			{
				throw new MoLangRuntimeException(
					"MoParams: Expected parameter type of " + typeof(T).Name + ", " + obj.GetType().Name + " given.",
					null);
			}
		}

		/// <summary>
		///		Check if the index as specified by <paramref name="index"/> is available
		/// </summary>
		/// <param name="index">The index to check availability for</param>
		/// <returns>
		///		True if the index is available
		/// </returns>
		public bool Contains(int index)
		{
			return _parameters.Length >= index + 1;
		}

		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as an <see cref="int"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public int GetInt(int index)
		{
			return (int)GetDouble(index);
		}
		
		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as a <see cref="double"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public double GetDouble(int index)
		{
			return Get<DoubleValue>(index).Value;
		}
		
		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as a <see cref="IMoStruct"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public IMoStruct GetStruct(int index)
		{
			return Get<IMoStruct>(index);
		}
		
		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as a <see cref="string"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public string GetString(int index)
		{
			return Get<StringValue>(index).Value;
		}
		
		/// <summary>
		///		Gets the parameter at <paramref name="index"/> and returns its value as a <see cref="MoLangEnvironment"/>
		/// </summary>
		/// <param name="index">The index of the parameter to retrieve</param>
		/// <returns>
		///		The value at specified index
		/// </returns>
		public MoLangEnvironment GetEnv(int index)
		{
			return Get<MoLangEnvironment>(index);
		}

		/// <summary>
		///		Get all parameters passed as an array
		/// </summary>
		/// <returns>
		///		An array of <see cref="IMoValue"/>
		/// </returns>
		public IMoValue[] GetParams()
		{
			return _parameters;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			var values = GetParams();

			if (values != null && values.Length > 0)
			{
				return string.Join(',', values.Select(x => x.AsString()));
			}

			return base.ToString();
		}
	}
}