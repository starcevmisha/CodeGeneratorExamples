using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace ConsoleApp3.Benchmarks
{
	public class BenchmarkAssign
	{
		private static Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{"Id", "5"},
			{"Name", "Misha"},
			{"Date", "05.01.1999"}
		};

		public static Func<Dictionary<string, object>, object> Emit =
			ReflectionEmitExample.GenerateMethod(typeof(Person));

		public static Func<Dictionary<string, object>, object> ReflectionGenerate =
			ReflectionExample.GenerateMethod(typeof(Person));

		[Benchmark]
		public static Person assignTargetEmit() => (Person)Emit(dictionary);

		[Benchmark]
		public static Person assignPersonReflection() => (Person)ReflectionGenerate(dictionary);
	}
}