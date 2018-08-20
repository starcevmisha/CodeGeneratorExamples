using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace ConsoleApp3.Benchmarks
{
	public class BenchamrkGeneration<T>
	{
		[Benchmark]
		public static Func<Dictionary<string, object>, object> ExpressionTreeGenerate() =>
			ExpressionTreeExample.GenerateMethod(typeof(T));

		[Benchmark]
		public static Func<Dictionary<string, object>, object> ReflectionGenerate() =>
			ReflectionExample.GenerateMethod(typeof(T));

		[Benchmark]
		public static Func<Dictionary<string, object>, object> roslynRaw() =>
			RoslynRawExample.GenerateMethod(typeof(T));

		[Benchmark]
		public static Func<Dictionary<string, object>, object> RoslynSb() =>
			RoslynWithStringBuilder.GenerateMethod(typeof(T));

		[Benchmark]
		public static Func<Dictionary<string, object>, object> EmitGenerate() =>
			ReflectionEmitExample.GenerateMethod(typeof(T));

		[Benchmark]
		public static Func<Dictionary<string, object>, object> Gremit() =>
			GremitExample.GenerateMethod(typeof(T));
	}

}