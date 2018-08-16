using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ConsoleApp3
{
	public class ExpressionTreeExample
	{
		public static Expression<Func<int, bool>> SimpleExpression()
		{
			var left = Expression.Parameter(typeof(int), "num");
			var right = Expression.Constant(5, typeof(int));
			var numLessThanFive = Expression.LessThan(left, right);

			return Expression.Lambda<Func<int, bool>>(numLessThanFive, left);
		}

		public static Func<Dictionary<string, object>, object> GenerateMethod(
			Type entityType)
		{
			var inputParameterType = typeof(Dictionary<string, object>);
			var tryGetValueMethod = inputParameterType.GetMethod("TryGetValue");
			var inputParameter = Expression.Parameter(inputParameterType, "dictionary");

			var properties = entityType.GetProperties();

			var list = new List<Expression>();

			var result = Expression.Variable(entityType, "result");
			var newEntityType = Expression.New(entityType);
			var assignResult = Expression.Assign(result, newEntityType);
			list.Add(assignResult);

			var outValue = Expression.Variable(typeof(object), "value");


			foreach (var property in properties)
			{
				var callTryGetValueMethod = Expression.Call(inputParameter, tryGetValueMethod,
					Expression.Constant(property.Name), outValue);
				var typedOutValue = Expression.Convert(outValue, property.PropertyType);

				var access = Expression.MakeMemberAccess(result, property);
				var assign = Expression.Assign(access, typedOutValue);

				var ifBlock = Expression.IfThen(callTryGetValueMethod, assign);
				list.Add(ifBlock);
			}

			list.Add(result);

			var block = Expression.Block(new[] {outValue, result}, list);
			var lambda = Expression.Lambda<Func<Dictionary<string, object>, object>>(block, inputParameter);
			return lambda.Compile();
		}
	}
}