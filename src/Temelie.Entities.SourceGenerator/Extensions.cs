using System.Text;
using Microsoft.CodeAnalysis;

namespace Temelie;
internal static class Extensions
{
    internal static string FullName(this ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.CSharpErrorMessageFormat);
    }

    internal static string FullNamespace(this INamespaceOrTypeSymbol type)
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

    internal static bool TryGetNonNullable(this ITypeSymbol symbol, out ITypeSymbol nonNullable)
    {
        if (symbol.NonNullableValueType() is { } t)
        {
            nonNullable = t;
            return true;
        }

        if (symbol.NullableAnnotation.IsNullable())
        {
            nonNullable = symbol.WithNullableAnnotation(NullableAnnotation.NotAnnotated);
            return true;
        }

        nonNullable = default;
        return false;
    }

    internal static bool IsNullable(this ITypeSymbol symbol) => symbol.IsNullableReferenceType() || symbol.IsNullableValueType();

    internal static bool IsNullableReferenceType(this ITypeSymbol symbol) => symbol.NullableAnnotation.IsNullable();

    internal static bool IsNullableValueType(this ITypeSymbol symbol) => symbol.NonNullableValueType() != null;

    internal static ITypeSymbol NonNullableValueType(this ITypeSymbol symbol)
    {
        if (symbol.IsValueType && symbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } namedType)
        {
            return namedType.TypeArguments[0];
        }

        return null;
    }

    internal static bool IsNullableUpgraded(this ITypeSymbol symbol)
    {
        if (symbol.NullableAnnotation == NullableAnnotation.None)
        {
            return false;
        }

        return symbol switch
        {
            INamedTypeSymbol namedTypeSymbol when namedTypeSymbol.TypeArguments.Any(x => !x.IsNullableUpgraded()) => false,
            IArrayTypeSymbol arrayTypeSymbol when !arrayTypeSymbol.ElementType.IsNullableUpgraded() => false,
            _ => true
        };
    }

    internal static bool IsNullable(this NullableAnnotation nullable) =>
        nullable is NullableAnnotation.Annotated or NullableAnnotation.None;

}
