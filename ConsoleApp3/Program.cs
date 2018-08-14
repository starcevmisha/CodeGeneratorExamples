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
	internal class Person
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public DateTime Date { get; set; }
	}

	internal class Program
	{
		private static Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{"Id", 5},
			{"Name", "Misha"},
			{"Date", new DateTime(1999, 5, 1)}
		};

		public static object MapDictionaryToType1(Dictionary<string, object> dictionary)
		{
			var person = new Person();
			object value;
			if (dictionary.TryGetValue("Id", out value))
			{
				person.Id = (int) value;
			}

			if (dictionary.TryGetValue("Name", out value))
			{
				person.Name = (string) value;
			}

			if (dictionary.TryGetValue("Date", out value))
			{
				person.Date = (DateTime) value;
			}

			return person;
		}

		private static void Main(string[] args)
		{
			var lambda1 = SimpleExpression();
			//                        SaveLambda(lambda1);

			var lambda2 = BuildDictionaryToTypeExpression(typeof(Person));

			//            SaveLambda(lambda2);
			RoslynExampleGenerateMethodWithCSharpSyntaxFactory(typeof(Person));
			var person = lambda2.Compile()(dictionary);
			var personReflection = MapDictionaryToTypeReflection(typeof(Person), dictionary);
			var personHM = MapDictionaryToTypeHandMade(dictionary);
			var b = 67;
		}

		public static void RoslynExampleGenerateMethodWithCSharpSyntaxFactory(Type type)
		{
			var ifBlock = new StringBuilder();
			foreach (var property in type.GetProperties())
			{
				ifBlock.AppendFormat(@"if (dictionary.TryGetValue(""{0}"", out value))
				{{
					target.{0} = ({1})value;
				}}
				", property.Name, property.PropertyType.Name);
			}

			var code = $@"public static object MapDictionaryToType1(Dictionary<string, object> dictionary)
			{{
				var target = new {type.Name}();
				object value;
				return person;
				{ifBlock}
			}}
			";
			Console.WriteLine(code);
		}

		public static void RoslynExampleGenerateMethod(Type type)
		{
			//var person = new Person();
			var typeIdentifier = SyntaxFactory.IdentifierName(type.Name);
			var personDeclaration = SyntaxFactory.LocalDeclarationStatement(
				SyntaxFactory.VariableDeclaration(typeIdentifier)
					.AddVariables(
						SyntaxFactory.VariableDeclarator("target")
							.WithInitializer(
								SyntaxFactory.EqualsValueClause(
									SyntaxFactory.ObjectCreationExpression(
										typeIdentifier, SyntaxFactory.ArgumentList(), null))
							)
					)
			);
			//object value;
			var outValueDeclaration = SyntaxFactory.LocalDeclarationStatement(
				SyntaxFactory.VariableDeclaration(SyntaxFactory.IdentifierName("object"))
					.AddVariables(SyntaxFactory.VariableDeclarator("value")));
			// person.Id = (int) value;
			var assign = SyntaxFactory.ExpressionStatement(
				SyntaxFactory.AssignmentExpression(
					SyntaxKind.SimpleAssignmentExpression,
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.IdentifierName("target"),
						SyntaxFactory.IdentifierName("Id")
					),
					SyntaxFactory.CastExpression(
						SyntaxFactory.ParseTypeName("int"),
						SyntaxFactory.IdentifierName("value")
					)
				)
			);
			//if (dictionary.TryGetValue("Id", out value))
			var ifBlock = SyntaxFactory.IfStatement(
				SyntaxFactory.InvocationExpression(
					SyntaxFactory.MemberAccessExpression(
						SyntaxKind.SimpleMemberAccessExpression,
						SyntaxFactory.IdentifierName("dictionary"),
						SyntaxFactory.IdentifierName("TryGetValue")
					)
				).WithArgumentList(SyntaxFactory.ArgumentList(
					SyntaxFactory.SingletonSeparatedList<ArgumentSyntax>(
						SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
							SyntaxFactory.Literal("Id")))))),
				assign);

			var block = SyntaxFactory.Block(personDeclaration, outValueDeclaration, ifBlock);
			var methodDeclaration = SyntaxFactory
				.MethodDeclaration(SyntaxFactory.ParseTypeName("object"), "Foo")
				.AddParameterListParameters(
					SyntaxFactory.Parameter(
							SyntaxFactory.Identifier("dictionary"))
						.WithType(SyntaxFactory.ParseTypeName("Dictionary<string, object>")))
				.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword),
					SyntaxFactory.Token(SyntaxKind.StaticKeyword))
				.WithBody(block);

			// Output new code to the console.
			var code = methodDeclaration
				.NormalizeWhitespace()
				.ToFullString();
			Console.WriteLine(code);
		}

		public static Expression<Func<int, bool>> SimpleExpression()
		{
			var left = Expression.Parameter(typeof(int), "num");
			var right = Expression.Constant(5, typeof(int));
			var numLessThanFive = Expression.LessThan(left, right);

			return Expression.Lambda<Func<int, bool>>(numLessThanFive, left);
		}

		private static void SaveLambda<T>(Expression<T> lambda2)
		{
			var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
				new AssemblyName("dyn"), // call it whatever you want
				AssemblyBuilderAccess.Save);

			var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
			var dt = dm.DefineType("dyn_type");
			var method = dt.DefineMethod(
				"Foo",
				MethodAttributes.Public | MethodAttributes.Static);

			lambda2.CompileToMethod(method);
			dt.CreateType();

			da.Save("dyn.dll");
		}

		public static object MapDictionaryToTypeReflection(Type type,
			Dictionary<string, object> dictionary)
		{
			var typeInstance = Activator.CreateInstance(type);

			var properties = type.GetProperties();
			foreach (var property in properties)
				if (dictionary.TryGetValue(property.Name, out var value))
					property.SetValue(typeInstance, value);
			return typeInstance;
		}

		public static Expression<Func<Dictionary<string, object>, object>> BuildDictionaryToTypeExpression(
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
			return lambda;
		}

		public static object MapDictionaryToTypeHandMade(Dictionary<string, object> dictionary)
		{
			var person = new Person();
			object value;
			if (dictionary.TryGetValue("Id", out value))
				person.Id = (int) value;
			if (dictionary.TryGetValue("Name", out value))
				person.Name = (string) value;
			if (dictionary.TryGetValue("Date", out value))
				person.Date = (DateTime) value;
			return person;
		}
	}
}