using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace ConsoleApp3
{
	public static class Compiler
	{
		public static Assembly CompileAndLoad(SyntaxTree st)
		{
			var compilation
				= CSharpCompilation.Create("TestRoslyn.dll", new[] { st }, null, DefaultCompilationOptions);
			compilation = compilation.WithReferences(DefaultReferences);
			using (var stream = new MemoryStream())
			{
				EmitResult result = compilation.Emit(stream);
				if (result.Success)
				{
					var assembly = Assembly.Load(stream.GetBuffer());
					return assembly;
				}

				return null;
			}
		}


		private static readonly CSharpCompilationOptions DefaultCompilationOptions =
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);


		private static readonly IEnumerable<MetadataReference> DefaultReferences =
			new []{
				MetadataReference.CreateFromFile(typeof(Person).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(System.GenericUriParser).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).Assembly
					.Location)
			};
	}
}