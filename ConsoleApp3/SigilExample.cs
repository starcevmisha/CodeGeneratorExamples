using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using GrEmit;
using Sigil;
namespace ConsoleApp3
{
	public class SigilExample
	{
		public static Func<Dictionary<string, object>, object> GenerateMethod(Type type)
		{
			var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName("dyn"), // call it whatever you want
				AssemblyBuilderAccess.RunAndSave);

			var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
			var dt = dm.DefineType("dyn_type");



			var emiter = Emit<Func<int>>.NewDynamicMethod("MyMethod");

			var method = dt.DefineMethod(
				"Foo",
				MethodAttributes.Public | MethodAttributes.Static, typeof(object),
				new[] {typeof(Dictionary<string, object>)});
			method.DefineParameter(1, ParameterAttributes.None, "dictionary");


			using (var il = new GroboIL(method))
			{
				var target = il.DeclareLocal(type);
				var value = il.DeclareLocal(typeof(object));

				il.Newobj(type.GetConstructor(Type.EmptyTypes)); // [Person]
				il.Stloc(target); // []
				foreach (var property in type.GetProperties())
				{
					var label = il.DefineLabel("ifLabel");

					il.Ldarg(0); // [Dictionary<String, Object>]
					il.Ldstr(property.Name); // [Dictionary<String, Object>, String]
					il.Ldloca(value); // [Dictionary<String, Object>, String, Object&]
					il.Call(typeof(Dictionary<string, object>)
						.GetMethod("TryGetValue")); // [Boolean]

					il.Brfalse(label); // []

					il.Ldloc(target); // [Person]
					il.Ldloc(value); // [Person, Object]
					il.Castclass(typeof(string)); // [Dictionary<String, Object>, String]
					il.Call(property.GetSetMethod(true)); // []

					il.MarkLabel(label);
				}

				il.Ldloc(target);
				il.Ret();
				Console.WriteLine(il.GetILCode());
			}


			dt.CreateType();
			da.Save("dyn.dll");


			return (dic) => dt.GetMethod("Foo").Invoke(null, new object[] {dic});
		}
	}
}