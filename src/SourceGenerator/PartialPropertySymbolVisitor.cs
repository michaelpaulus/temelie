using Microsoft.CodeAnalysis;

namespace Temelie.SourceGenerator;

public class PartialPropertySymbolVisitor : SymbolVisitor
{
    private readonly CancellationToken _cancellationToken;

    public PartialPropertySymbolVisitor(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    private readonly IList<PartialPropertyModel> _partialPropertyModels = new List<PartialPropertyModel>();

    public IEnumerable<PartialPropertyModel> PartialPropertyModels => _partialPropertyModels;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            _cancellationToken.ThrowIfCancellationRequested();
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            _cancellationToken.ThrowIfCancellationRequested();
            member.Accept(this);
        }
    }

    public override void VisitProperty(IPropertySymbol symbol)
    {
        if (symbol.IsPartialDefinition)
        {
            var propertyName = symbol.Name;
            var declaringClass = symbol.ContainingType?.Name;
            var declaringNamespace = symbol.ContainingNamespace?.ToDisplayString();
            _partialPropertyModels.Add(new PartialPropertyModel(declaringNamespace, declaringClass, propertyName));
        }
        base.VisitProperty(symbol);
    }

}
