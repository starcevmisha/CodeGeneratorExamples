using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace ConsoleApp3
{
	public static class ReflectionEmitExample
	{
		public static Func<Dictionary<string, object>, object> GenerateMethod(Type type)
		{
			var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName("dyn"), // call it whatever you want
				AssemblyBuilderAccess.Save);

			var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
			var dt = dm.DefineType("dyn_type");

			var method = dt.DefineMethod(
				"Foo",
				MethodAttributes.Public | MethodAttributes.Static, typeof(object), new[] { typeof(Dictionary<string, object>) });
			method.DefineParameter(1, ParameterAttributes.None, "dictionary");



			var generator = method.GetILGenerator();

			var target = generator.DeclareLocal(type);
			var value = generator.DeclareLocal(typeof(object));
//			var isRepresent = generator.DeclareLocal(typeof(bool));

			var label_39 = generator.DefineLabel();


			generator.Emit(OpCodes.Newobj, typeof(ConsoleApp3.Person).GetConstructor(Type.EmptyTypes));//Создаём объект типа Person
			generator.Emit(OpCodes.Stloc_0);


			generator.Emit(OpCodes.Ldarg_0);// загружаем dictionary
			generator.Emit(OpCodes.Ldstr, "Id");// строка
			generator.Emit(OpCodes.Ldloca_S, value); // value
			generator.Emit(OpCodes.Callvirt, typeof(Dictionary<string, object>).GetMethod("TryGetValue"));// на стеке будет лежать, true or false
			generator.Emit(OpCodes.Brfalse_S, label_39);// если нет такого с словаре, то идём дальше

			generator.Emit(OpCodes.Ldloc_0);// Person 
			generator.Emit(OpCodes.Ldloc_1);// value
			generator.Emit(OpCodes.Unbox_Any, typeof(System.Int32)); // cast
			generator.Emit(OpCodes.Callvirt, typeof(ConsoleApp3.Person).GetProperty("Id").GetSetMethod(true)); //Устанавливаем значение

			generator.MarkLabel(label_39);
			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ret);


			dt.CreateType();
			da.Save("dyn.dll");

			return null;
		}
	}
}