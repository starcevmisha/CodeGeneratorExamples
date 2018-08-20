using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace ConsoleApp3.Benchmarks
{
	public partial class BenchmarkAssign10000Property
	{

		public static Func<Dictionary<string, object>, object> Emit =
			ReflectionEmitExample.GenerateMethod(typeof(BigTarget));

		public static Func<Dictionary<string, object>, object> ReflectionGenerate =
			ReflectionExample.GenerateMethod(typeof(BigTarget));

		[Benchmark]
		public static BigTarget assignTargetEmit() => (BigTarget)Emit(dictionary);

		[Benchmark]
		public static BigTarget assignPersonReflection() => (BigTarget)ReflectionGenerate(dictionary);
	}
}