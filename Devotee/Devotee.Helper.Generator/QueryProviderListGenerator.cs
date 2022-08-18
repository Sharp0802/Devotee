using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Devotee.Helper.Generator;

[Generator]
public class QueryProviderListGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        IEnumerable<INamedTypeSymbol> GetAllNamedTypeSymbols(INamespaceSymbol ns)
        {
            foreach (var nts in ns.GetTypeMembers())
                yield return nts;
            foreach (var ins in ns.GetNamespaceMembers())
            foreach (var nts in GetAllNamedTypeSymbols(ins))
                yield return nts;
        }
        
        var arrVal = context.Compilation.SourceModule.ReferencedAssemblySymbols
                            .SelectMany(asm => GetAllNamedTypeSymbols(asm.GlobalNamespace))
                            .Where(nts =>
                                // must subclass of IQueryProvider contained in assembly Devotee.Core
                                nts.AllInterfaces.Any(ints => ints.Name == "IQueryProvider") &&
                                // must non-generic non-abstract class
                                !nts.IsGenericType &&
                                !nts.IsAbstract)
                            .Aggregate(new StringBuilder(), (builder, nts) =>
                                builder.Append("new global::")
                                       .Append(nts.ToDisplayString())
                                       .Append("(),\n"))
                            .ToString();

        var source = @$"
using IQueryProvider = Devotee.Core.Interfaces.IQueryProvider;

namespace Devotee.Helper;

public static class QueryProvider
{{
    public static IQueryProvider[] QueryProviders = 
    {{
        {arrVal}    }};
}}
";
        
        context.AddSource("QueryProvider.g.cs", SourceText.From(source, Encoding.UTF8));
    }
}