using Microsoft.CodeAnalysis;

namespace Temelie.DependencyInjection.SourceGenerator;

public class DependencyInjectionSymbolVisitor : SymbolVisitor
{

    public DependencyInjectionSymbolVisitor(CancellationToken token)
    {
        _token = token;
    }

    private readonly IList<Export> _symbols = new List<Export>();
    private readonly CancellationToken _token;

    public IEnumerable<Export> Exports => _symbols;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            _token.ThrowIfCancellationRequested();
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        _token.ThrowIfCancellationRequested();

        foreach (var attribute in symbol.GetAttributesWithInherited())
        {
            if (!symbol.IsAbstract &&
                (
                    attribute.AttributeClass.Name == "ExportHostedServiceAttribute" ||
                    attribute.AttributeClass.Name == "ExportStartupConfigurationAttribute" ||
                    attribute.AttributeClass.Name == "ExportProviderAttribute" ||
                    attribute.AttributeClass.Name == "ExportSingletonAttribute" ||
                    attribute.AttributeClass.Name == "ExportScopedAttribute" ||
                    attribute.AttributeClass.Name == "ExportTransientAttribute"
                ))
            {
                var forType = "";

                if (attribute.ConstructorArguments.Length == 1)
                {
                    var forType1 = (attribute.ConstructorArguments[0].Value as INamedTypeSymbol);
                    if (forType1 is not null)
                    {
                        forType = forType1.FullName();
                    }
                }

                var priority = int.MaxValue;
                var type = symbol.FullName();

                foreach (var arg in attribute.NamedArguments)
                {
                    if (arg.Key == "Priority")
                    {
                        priority = Convert.ToInt32(arg.Value.Value);
                    }
                    if (arg.Key == "Type")
                    {
                        type = (arg.Value.Value as INamedTypeSymbol)?.FullName();
                    }
                }

                var export = new Export()
                {
                    IsProvider = attribute.AttributeClass.Name == "ExportProviderAttribute",
                    IsSingleton = attribute.AttributeClass.Name == "ExportSingletonAttribute",
                    IsScoped = attribute.AttributeClass.Name == "ExportScopedAttribute",
                    IsTransient = attribute.AttributeClass.Name == "ExportTransientAttribute",
                    IsHosted = attribute.AttributeClass.Name == "ExportHostedServiceAttribute",
                    IsStartupConfig = attribute.AttributeClass.Name == "ExportStartupConfigurationAttribute",
                    Type = type,
                    ForType = forType,
                    Priority = priority
                };

                _symbols.Add(export);
            }
        }

    }

}
