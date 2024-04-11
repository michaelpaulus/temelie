using Microsoft.CodeAnalysis;

namespace Cornerstone.DependencyInjection.SourceGenerator;

public class DependencyInjectionSymbolVisitor : SymbolVisitor
{

    public DependencyInjectionSymbolVisitor()
    {
    }

    private readonly IList<INamedTypeSymbol> _symbols = new List<INamedTypeSymbol>();
    public IEnumerable<INamedTypeSymbol> Symbols => _symbols;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "ExportProviderAttribute" ||
        ad.AttributeClass.Name == "ExportSingletonAttribute" ||
        ad.AttributeClass.Name == "ExportTransientAttribute");
        if (attribute != null)
        {
            _symbols.Add(symbol);
        }
    }

}
