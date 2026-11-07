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
            var attributeName = attribute.AttributeClass?.Name;
            if (!symbol.IsAbstract &&
                (
                    attributeName == "ExportHostedServiceAttribute" ||
                    attributeName == "ExportStartupConfigurationAttribute" ||
                    attributeName == "ExportProviderAttribute" ||
                    attributeName == "ExportSingletonAttribute" ||
                    attributeName == "ExportScopedAttribute" ||
                    attributeName == "ExportTransientAttribute"
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
                        type = (arg.Value.Value as INamedTypeSymbol)?.FullName() ?? type;
                    }
                }

                var export = new Export()
                {
                    IsProvider = attributeName == "ExportProviderAttribute",
                    IsSingleton = attributeName == "ExportSingletonAttribute",
                    IsScoped = attributeName == "ExportScopedAttribute",
                    IsTransient = attributeName == "ExportTransientAttribute",
                    IsHosted = attributeName == "ExportHostedServiceAttribute",
                    IsStartupConfig = attributeName == "ExportStartupConfigurationAttribute",
                    Type = type,
                    ForType = forType,
                    Priority = priority
                };

                _symbols.Add(export);
            }
        }

    }

}
