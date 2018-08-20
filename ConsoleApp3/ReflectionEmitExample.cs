using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

// ReSharper disable AssignNullToNotNullAttribute

namespace ConsoleApp3
{
	public static class ReflectionEmitExample
	{
		public delegate object Generate(Dictionary<string, object> dictionary);

		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public static Func<Dictionary<string, object>, object> GenerateMethod(Type type)
		{

			var method = new DynamicMethod("",
				typeof(object),
				new[] { typeof(Dictionary<string, object>) });

			var generator = method.GetILGenerator();

			var target = generator.DeclareLocal(type);
			var value = generator.DeclareLocal(typeof(object));


			generator.Emit(OpCodes.Newobj, type.GetConstructor(Type.EmptyTypes)); //Создаём объект типа Person
			generator.Emit(OpCodes.Stloc_0); // Сохряняем в лист локальных перменных

			foreach (var property in type.GetProperties())
			{
				var label = generator.DefineLabel();
				generator.Emit(OpCodes.Ldarg_0); // dictionary
				generator.Emit(OpCodes.Ldstr, property.Name); // строка
				generator.Emit(OpCodes.Ldloca_S, value); // адрес value
				generator.Emit(OpCodes.Callvirt,
					typeof(Dictionary<string, object>)
						.GetMethod("TryGetValue")); // на стеке будет лежать, true or false
				generator.Emit(OpCodes.Brfalse_S, label); // если нет такого с словаре, то идём дальше


				generator.Emit(OpCodes.Ldloc_0); // Person 
				generator.Emit(OpCodes.Ldloc_1); // value
				generator.Emit(OpCodes.Castclass, typeof(string));
				generator.Emit(OpCodes.Callvirt, property.GetSetMethod(true)); //Устанавливаем значение

				generator.MarkLabel(label);
			}

			generator.Emit(OpCodes.Ldloc_0);
			generator.Emit(OpCodes.Ret);

			return (Func<Dictionary<string, object>, object>) method
				.CreateDelegate(typeof(Func<Dictionary<string, object>, object>));
		}
	}
}