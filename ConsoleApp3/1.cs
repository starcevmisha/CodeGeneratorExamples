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
				target.Id = (Int32)value;
			}
//			if (dictionary.TryGetValue("Name", out value))
//			{
//				target.Name = (String)value;
//			}
//			if (dictionary.TryGetValue("Date", out value))
//			{
//				target.Date = (DateTime)value;
//			}

			return target;
		}
	}
}