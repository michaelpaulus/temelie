using System.Text;
using Microsoft.CodeAnalysis;

namespace Temelie.DependencyInjection.SourceGenerator;
public static class Extensions
{
    public static string FullName(this ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }

    public static string FullNamespace(this INamespaceOrTypeSymbol type)
    {
        var value = new StringBuilder();

        var ns = type.ContainingNamespace;

        while (ns is not null)
        {
            if (value.Length > 0)
            {
                value.Append(".");
            }
            value.Append(ns.Name);
            ns = ns.ContainingNamespace;
        }

        return value.ToString();
    }

    public static IEnumerable<AttributeData> GetAttributesWithInherited(this INamedTypeSymbol typeSymbol)
    {
        foreach (var attribute in typeSymbol.GetAttributes())
        {
            yield return attribute;
        }

        var baseType = typeSymbol.BaseType;
        while (baseType != null)
        {
            foreach (var attribute in baseType.GetAttributes())
            {
                if (IsInherited(attribute))
                {
                    yield return attribute;
                }
            }

            baseType = baseType.BaseType;
        }
    }

    private static bool IsInherited(this AttributeData attribute)
    {
        if (attribute.AttributeClass == null)
        {
            return false;
        }

        foreach (var attributeAttribute in attribute.AttributeClass.GetAttributes())
        {
            var @class = attributeAttribute.AttributeClass;
            if (@class != null && @class.Name == nameof(AttributeUsageAttribute) &&
                @class.ContainingNamespace?.Name == "System")
            {
                foreach (var kvp in attributeAttribute.NamedArguments)
                {
                    if (kvp.Key == nameof(AttributeUsageAttribute.Inherited))
                    {
                        return (bool)kvp.Value.Value!;
                    }
                }

                // Default value of Inherited is true
                return true;
            }
        }

        // An attribute without an `AttributeUsage` attribute will also default to being inherited.
        return true;
    }
}
