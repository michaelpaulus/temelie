using Microsoft.CodeAnalysis;

namespace Temelie.DependencyInjection.SourceGenerator;

public class DependencyInjectionSymbolVisitor : SymbolVisitor
{

    public DependencyInjectionSymbolVisitor()
    {
    }

    private readonly IList<Export> _symbols = new List<Export>();
    public IEnumerable<Export> Exports => _symbols;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            if (attribute.AttributeClass.Name == "ExportHostedServiceAttribute" ||
                attribute.AttributeClass.Name == "ExportStartupConfigurationAttribute" ||
                attribute.AttributeClass.Name == "ExportProviderAttribute" ||
                attribute.AttributeClass.Name == "ExportSingletonAttribute" ||
                attribute.AttributeClass.Name == "ExportTransientAttribute")
            {
                var forType = "";

                if (attribute.ConstructorArguments.Length == 1)
                {
                    forType = (attribute.ConstructorArguments[0].Value as INamedTypeSymbol)?.FullName();
                }

                var priority = int.MaxValue;

                foreach (var arg in attribute.NamedArguments)
                {
                    if (arg.Key == "Priority")
                    {
                        priority = Convert.ToInt32(arg.Value.Value);
                    }
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
