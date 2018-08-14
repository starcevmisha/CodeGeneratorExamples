using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;

namespace ConsoleApp3
{
	public class RoslynWithStringBuilder
	{
		public static Func<Dictionary<string, object>, object>
			GenerateMethod(Type type)
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

			var code = $@"
using System;
using System.Collections.Generic;
using {type.Namespace};
namespace CodeGenerationSample{{
public static class StringBuilderCodeGeneration{{
public static object MapDictionary(Dictionary<string, object> dictionary)
{{
	var target = new {type.Name}();
	object value;
	{ifBlock}
	return target;
}}
}}
}}
	";

			var syntaxTree = CSharpSyntaxTree.ParseText(code);
			var assembly = Compiler.CompileAndLoad(syntaxTree);

			var typeA = assembly.GetType("CodeGenerationSample.StringBuilderCodeGeneration");
			return dic =>
				typeA.InvokeMember("MapDictionary",
					BindingFlags.Default | BindingFlags.InvokeMethod,
					null,
					typeA,
					new object[] {dic});
		}
	}
}