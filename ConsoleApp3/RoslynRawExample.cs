using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Microsoft.CodeAnalysis.CSharp.SyntaxKind;

namespace ConsoleApp3
{

	public class RoslynRawExample
	{
		public static Func<Dictionary<string, object>, object> 
			GenerateMethod(Type type)
		{
			var listStatementSyntax = new List<StatementSyntax>();

			//var person = new Person();
			var typeIdentifier = IdentifierName(type.Name);
			var personDeclaration = LocalDeclarationStatement(
				VariableDeclaration(typeIdentifier)
					.AddVariables(
						VariableDeclarator("target")
							.WithInitializer(
								EqualsValueClause(
									ObjectCreationExpression(
										typeIdentifier, ArgumentList(), null))
							)
					)
			);
			listStatementSyntax.Add(personDeclaration);

			//object value;
			var outValueDeclaration = LocalDeclarationStatement(
				VariableDeclaration(PredefinedType(Token(ObjectKeyword)))
					.AddVariables(VariableDeclarator("value")));
			listStatementSyntax.Add(outValueDeclaration);


			foreach (var property in type.GetProperties())
			{
				// person.Id = (int) value;
				var assign = ExpressionStatement(
					AssignmentExpression(
						SimpleAssignmentExpression,
						MemberAccessExpression(
							SimpleMemberAccessExpression,
							IdentifierName("target"),
							IdentifierName(property.Name)
						),
						CastExpression(
							ParseTypeName("string"),
							IdentifierName("value")
						)
					)
				);

				//if (dictionary.TryGetValue("Id", out value))
				listStatementSyntax.Add(
					IfStatement(
						InvocationExpression(
							MemberAccessExpression(
								SimpleMemberAccessExpression,
								IdentifierName("dictionary"),
								IdentifierName("TryGetValue")
							)
						).WithArgumentList(
							ArgumentList(
								SeparatedList<ArgumentSyntax>(
									new SyntaxNodeOrToken[]
									{
										Argument(LiteralExpression(StringLiteralExpression,
											Literal(property.Name))),
										Token(CommaToken),
										Argument(IdentifierName("value"))
											.WithRefKindKeyword(Token(OutKeyword))
									}
								)
							)
						),
						assign));
			}

			listStatementSyntax.Add(ReturnStatement(
				IdentifierName("target")));

			var block = Block(listStatementSyntax);
			var methodDeclaration = MethodDeclaration(ParseTypeName("object"), "MapDictionary")
				.AddParameterListParameters(
					Parameter(
							Identifier("dictionary"))
						.WithType(ParseTypeName("Dictionary<string, object>")))
				.AddModifiers(Token(PublicKeyword),
					Token(StaticKeyword))
				.WithBody(block);


			var @namespace = CompilationUnit()
				.AddUsings(
					UsingDirective(ParseName("System")),
					UsingDirective(ParseName("System.Collections.Generic")),
					UsingDirective(ParseName(type.Namespace)))
				.WithMembers(
					SingletonList<MemberDeclarationSyntax>(
						ClassDeclaration("RoslynCodeGeneration")
							.AddModifiers(
								Token(PublicKeyword),
								Token(StaticKeyword))
							.AddMembers(methodDeclaration)
					));


			var assembly = Compiler.CompileAndLoad(@namespace.SyntaxTree);

			var typeWithMethod = assembly.GetType("RoslynCodeGeneration");
			var method = typeWithMethod.GetMethod("MapDictionary",
				BindingFlags.Public | BindingFlags.Static);


			return (Func<Dictionary<string, object>, object>)method.CreateDelegate(typeof(Func<Dictionary<string, object>, object>));
		}
	}
}