using System;
using System.Collections.Generic;

namespace ConsoleApp3
{
	public static class A
	{
		public static object MapDictionary(Dictionary<string, object> dictionary)
		{
			var target = new Person();
			object value;

			if (dictionary.TryGetValue("Id", out value))
			{
				target.Id = (string)value;
			}
			if (dictionary.TryGetValue("Name", out value))
			{
				target.Name = (string)value;
			}
			if (dictionary.TryGetValue("Date", out value))
			{
				target.Date = (string)value;
			}

			return target;
		}
	}
}