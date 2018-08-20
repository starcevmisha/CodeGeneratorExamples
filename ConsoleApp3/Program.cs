using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ConsoleApp3.Benchmarks;


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
		private static void Main(string[] args)
		{
//			BenchmarkRunner.Run<BenchamrkGeneration<Person>>(); //3 свойтсва
//			BenchmarkRunner.Run<BenchamrkGeneration<BigTarget>>(); //10 000 свойств
//
//			BenchmarkRunner.Run<BenchmarkAssign>(); //Словарь из трех свойств
//			BenchmarkRunner.Run<BenchmarkAssign10000Property>(); //Словарь из 10000 свойств

//			BenchmarkRunner.Run<DynamicMeyhodVsDynamicAssembly>();

			var watch = new Stopwatch();
			watch.Start();
			for (var i = 0; i <= 1000; i++)
			{
				DynamicMeyhodVsDynamicAssembly.DynamicAssembly();
			}

			Console.WriteLine(watch.ElapsedMilliseconds);

			watch = new Stopwatch();
			watch.Start();
			for (var i = 0; i <= 1000; i++)
			{
				DynamicMeyhodVsDynamicAssembly.DynamicMethod();
			}

			Console.WriteLine(watch.ElapsedMilliseconds);

		}
	}

	public class DynamicMeyhodVsDynamicAssembly
	{
		[Benchmark]
		public static void DynamicAssembly()
		{
			var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName("dyn"), // call it whatever you want
				AssemblyBuilderAccess.ReflectionOnly);

			var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
			var dt = dm.DefineType("dyn_type");
			var method = dt.DefineMethod(
				"Foo",
				MethodAttributes.Public | MethodAttributes.Static,
				typeof(bool), new[] {typeof(int)});
			GenerateMethod(method.GetILGenerator());

			dt.CreateType();
			dt.GetMethod(method.Name, new[] {typeof(int)});
		}

		[Benchmark]
		public static void DynamicMethod()
		{
			var method = new DynamicMethod("Foo", typeof(bool), new[] {typeof(int)});

			GenerateMethod(method.GetILGenerator());

			method.CreateDelegate(typeof(Func<int, bool>));
		}

		private static void GenerateMethod(ILGenerator ilGenerator)
		{
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Ldc_I4_5);
			ilGenerator.Emit(OpCodes.Clt);
			ilGenerator.Emit(OpCodes.Ret);
		}
	}
}