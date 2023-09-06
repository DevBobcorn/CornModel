using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using CraftSharp.Molang.Attributes;
using CraftSharp.Molang.Runtime.Value;

namespace CraftSharp.Molang.Runtime.Struct.Interop
{
	public class PropertyCache
	{
		public readonly IReadOnlyDictionary<string, ValueAccessor> Properties;

		public readonly IReadOnlyDictionary<string, Func<object, MoParams, IMoValue>> Functions;

		public PropertyCache(Type arg)
		{
			var properties = new Dictionary<string, ValueAccessor>(StringComparer.OrdinalIgnoreCase);
			var functions = new Dictionary<string, Func<object, MoParams, IMoValue>>(
				StringComparer.OrdinalIgnoreCase);
			
			ProcessMethods(arg, functions);
			ProcessProperties(arg, properties);

			Functions = functions;
			Properties = properties;
		}

		private static void ProcessMethods(IReflect type,
			IDictionary<string, Func<object, MoParams, IMoValue>> functions)
		{
			var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

			foreach (var method in methods)
			{
				var functionAttribute = method.GetCustomAttribute<MoFunctionAttribute>();

				if (functionAttribute == null)
					continue;

				foreach (var name in functionAttribute.Name)
				{
					if (functions.ContainsKey(name))
					{
						Debug.WriteLine($"Duplicate function \'{name}\' in {type.ToString()}");

						continue;
					}

					IMoValue ExecuteMolangFunction(object instance, MoParams mo)
					{
						var methodParams = method.GetParameters();
						IMoValue value = DoubleValue.Zero;

						object[] parameters = new object[methodParams.Length];
						//List<object> parameters = new List<object>();

						if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(MoParams))
						{
							parameters[0] = mo;
							//parameters.Add(mo);
						}
						else
						{
							for (var index = 0; index < methodParams.Length; index++)
							{
								var parameter = methodParams[index];

								if (!mo.Contains(index))
								{
									if (!parameter.IsOptional)
										throw new MissingMethodException($"Missing parameter: {parameter.Name}");

									break;
								}

								var t = parameter.ParameterType;

								if (t == typeof(MoParams))
								{
									parameters[index] = mo; //.Add(mo);
								}
								else if (t == typeof(int))
								{
									parameters[index] = mo.GetInt(index);
								}
								else if (t == typeof(double))
								{
									parameters[index] = mo.GetDouble(index);
								}
								else if (t == typeof(float))
								{
									parameters[index] = (float)mo.GetDouble(index);
								}
								else if (t == typeof(string))
								{
									parameters[index] = mo.GetString(index);
								}
								else if (typeof(IMoStruct).IsAssignableFrom(t))
								{
									parameters[index] = mo.GetStruct(index);
								}
								else if (typeof(MoLangEnvironment).IsAssignableFrom(t))
								{
									parameters[index] = mo.GetEnv(index);
								}
								else
								{
									throw new Exception("Unknown parameter type.");
								}

								//TODO: Continue.
							}
						}

						var result = method.Invoke(instance, parameters);

						if (result != null)
						{
							if (result is IMoValue moValue) return moValue;

							return MoValue.FromObject(result);
						}

						return value;
					}

					functions.Add(name, ExecuteMolangFunction);
				}
			}
		}

		private static void ProcessProperties(IReflect type, IDictionary<string, ValueAccessor> valueAccessors)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in properties)
			{
				foreach (var functionAttribute in prop.GetCustomAttributes<MoPropertyAttribute>())
				{
					if (valueAccessors.ContainsKey(functionAttribute.Name))
						continue;

					var accessor = new PropertyAccessor(prop);
					if (prop.GetCustomAttribute<MoObservableAttribute>() != null)
						accessor.Observable = true;

					valueAccessors.Add(functionAttribute.Name, accessor);
				}
			}

			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

			foreach (var prop in fields)
			{
				foreach (var functionAttribute in prop.GetCustomAttributes<MoPropertyAttribute>())
				{
					if (valueAccessors.ContainsKey(functionAttribute.Name))
						continue;
					
					var accessor = new FieldAccessor(prop);
					if (prop.GetCustomAttribute<MoObservableAttribute>() != null)
						accessor.Observable = true;

					valueAccessors.Add(functionAttribute.Name, accessor);
				}
			}
		}
	}
}