using Microsoft.CodeAnalysis;

namespace Cornerstone.DependencyInjection.SourceGenerator;

public class DependencyInjectionSymbolVisitor : SymbolVisitor
{

    public DependencyInjectionSymbolVisitor()
    {
    }

    private readonly IList<Export> _symbols = new List<Export>();
    public IEnumerable<Export> Symbols => _symbols;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        var attribute = symbol.GetAttributes().FirstOrDefault(ad =>
        ad.AttributeClass.Name == "ExportHostedServiceAttribute" ||
        ad.AttributeClass.Name == "ExportStartupConfigurationAttribute" ||
        ad.AttributeClass.Name == "ExportProviderAttribute" ||
        ad.AttributeClass.Name == "ExportSingletonAttribute" ||
        ad.AttributeClass.Name == "ExportTransientAttribute"
        );
        if (attribute != null)
        {

            if (attribute is not null)
            {
                var forType = (attribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol)?.FullName();
                var priority = int.MaxValue;
                if (attribute.NamedArguments.Any(i => i.Key == "Priority"))
                {
                    priority = Convert.ToInt32(attribute.NamedArguments.First(i => i.Key == "Priority").Value.Value);
                }

                var export = new Export()
                {
                    IsProvider = attribute.AttributeClass.Name == "ExportProviderAttribute",
                    IsSingleton = attribute.AttributeClass.Name == "ExportSingletonAttribute",
                    IsTransient = attribute.AttributeClass.Name == "ExportTransientAttribute",
                    IsHosted = attribute.AttributeClass.Name == "ExportHostedServiceAttribute",
                    IsStartupConfig = attribute.AttributeClass.Name == "ExportStartupConfigurationAttribute",
                    Type = symbol.FullName(),
                    ForType = forType,
                    Priority = priority
                };

                _symbols.Add(export);
            }



        }
    }

}
