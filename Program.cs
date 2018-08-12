using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ConsoleApp4
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

        private static void Main(string[] args)
        {
            var lambda1 = SimpleExpression();
//                        SaveLambda(lambda1);

            var lambda2 = BuildDictionaryToTypeExpression(typeof(Person));

            //            SaveLambda(lambda2);
            RoslynExampleGenerateClassAndMethod();

            var person = lambda2.Compile()(dictionary);
            var personReflection = MapDictionaryToTypeReflection(typeof(Person), dictionary);
            var personHM = MapDictionaryToTypeHandMade(dictionary);
            var b = 67;
        }

public static void RoslynExampleGenerateClassAndMethod()
{
    // Create a namespace: (namespace CodeGenerationSample)
    var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName("CodeGenerationSample"));

    // Add System using statement: (using System)
    @namespace = @namespace.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("System")));

    //  Create a class: (class Order)
    var classDeclaration = SyntaxFactory.ClassDeclaration("Order");

    // Add the public modifier: (public class Order)
    classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

    // Create a string variable: (bool canceled;)
    var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName("bool"))
        .AddVariables(SyntaxFactory.VariableDeclarator("canceled"));

    // Create a field declaration: (private bool canceled;)
    var fieldDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword));

    // Create a Property: (public int Quantity { get; set; })
    var propertyDeclaration = SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("int"), "Id")
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .AddAccessorListAccessors(
            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

    // Create a stament with the body of a method.
    var syntax = SyntaxFactory.ParseStatement("canceled = true;");

    // Create a method
    var methodDeclaration = SyntaxFactory
        .MethodDeclaration(SyntaxFactory.ParseTypeName("void"), "MarkAsCanceled")
        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
        .WithBody(SyntaxFactory.Block(syntax));

    // Add the field, the property and method to the class.
    classDeclaration = classDeclaration.AddMembers(fieldDeclaration, propertyDeclaration, methodDeclaration);

    // Add the class to the namespace.
    @namespace = @namespace.AddMembers(classDeclaration);

    // Normalize and get code as string.
    var code = @namespace
        .NormalizeWhitespace()
        .ToFullString();

    // Output new code to the console.
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