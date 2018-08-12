using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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
        private static void Main(string[] args)
        {
            var left = Expression.Parameter(typeof(int), "num");
            var right = Expression.Constant(5, typeof(int));
            var numLessThanFive = Expression.LessThan(left, right);
            var lambda1 = Expression.Lambda<Func<int, bool>>(numLessThanFive, left);

//            lambda1.Compile()(10);
            var lambda2 = BuildDictionaryToTypeExpression(typeof(Person));
            var c = 67;



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


            var dict = new Dictionary<string, object>
            {
                {"Id", 5},
                {"Name", "Misha"},
                {"Date", new DateTime(1999, 5, 1)}
            };
//            var person = lambda2(dict);
            var personReflection = MapDictionaryToTypeReflection(typeof(Person), dict);
            var personHM = MapDictionaryToTypeHandMade(dict);
            var b = 67;
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

        public static Expression<Func<Dictionary<string, object>, object>> BuildDictionaryToTypeExpression(Type entityType)
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
                var callTryGetValueMethod = Expression.Call(inputParameter, tryGetValueMethod, Expression.Constant(property.Name), outValue);
                var typedOutValue = Expression.Convert(outValue, property.PropertyType);

                var access = Expression.MakeMemberAccess(result, property);
                var assign = Expression.Assign(access,typedOutValue);

                var ifBlock = Expression.IfThen(callTryGetValueMethod, assign);
                list.Add(ifBlock);
            }

            list.Add(result);

            var block = Expression.Block(new []{outValue, result}, list);
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