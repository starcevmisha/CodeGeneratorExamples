using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ConsoleApp3
{
	public class Person
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public string Date { get; set; }
	}

	internal class Program
	{
		private static Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{"Id", "5"},
			{"Name", "Misha"},
			{"Date", "05.01.1999"}
		};

		public static object MapDictionaryToType1(Dictionary<string, object> dictionary)
		{
			var person = new Person();
			object value;
			if (dictionary.TryGetValue("Id", out value)) person.Id = (string)value;

			if (dictionary.TryGetValue("Name", out value)) person.Name = (string) value;

			if (dictionary.TryGetValue("Date", out value)) person.Date = (string)value;

			return person;
		}

		private static void Main(string[] args)
		{
//			var lambda1 = ExpressionTreeExample.SimpleExpression();
			//                        SaveLambda(lambda1);
//
//			var personReflection = ReflectionExample.MapDictionaryToTypeReflection(typeof(Person));
//			var expressonTree = ExpressionTreeExample.BuildDictionaryToTypeExpression(typeof(Person));
//			var rawRoslyn = RoslynRawExample.GenerateMethod(typeof(Person));
//			var sbRoslyn = RoslynWithStringBuilder.GenerateMethod(typeof(Person));
//			var reflectionEmit = ReflectionEmitExample.GenerateMethod(typeof(Person));

//			var pers = reflectionEmit(dictionary);
			var persGremit = GremitExample.GenerateMethod(typeof(Person))(dictionary);

			var personHM = MapDictionaryToTypeHandMade(dictionary);
		}


//		private static void DynamicAssembly()
//		{
//			var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
//				new AssemblyName("dyn"), // call it whatever you want
//				AssemblyBuilderAccess.Save);
//
//			var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
//			var dt = dm.DefineType("dyn_type");
//			var method = dt.DefineMethod(
//				"Foo",
//				MethodAttributes.Public | MethodAttributes.Static,
//				typeof(object), new[] {typeof(Dictionary<string, object>)});
//			method.DefineParameter(1, ParameterAttributes.None, "source");
//			GenerateMethod(method.GetILGenerator());
//			method.CreateDelegate(/*какой-то тип*/)
//			dt.CreateType();
//
//			da.Save("dyn.dll");
//		}
//
//		private static void DynamicMethodCreator()
//		{
//var method = new DynamicMethod("Foo", typeof(object), new[] {typeof(Dictionary<string, object>)});
//var prm = method.DefineParameter(1, ParameterAttributes.None, "source");
//
//GenerateMapMethodBody(method.GetILGenerator());
//method.CreateDelegate(/*какой-то тип*/)
//		}


		public static object MapDictionaryToTypeHandMade(Dictionary<string, object> dictionary)
		{
			var person = new Person();
			object value;
			if (dictionary.TryGetValue("Id", out value))
				person.Id = (string)value;
			if (dictionary.TryGetValue("Name", out value))
				person.Name = (string) value;
			if (dictionary.TryGetValue("Date", out value))
				person.Date = (string)value;
			return person;
		}
	}
}