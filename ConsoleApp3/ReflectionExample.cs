using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp3
{
	public class ReflectionExample
	{
		public static Func<Dictionary<string, object>, object> GenerateMethod(Type type)
		{
			return (dic) =>
			{
				var typeInstance = Activator.CreateInstance(type);

				var properties = type.GetProperties();
				foreach (var property in properties)
					if (dic.TryGetValue(property.Name, out var value))
						property.SetValue(typeInstance, value);
				return typeInstance;
			};
		}
	}
}