using System.Text;
using Microsoft.CodeAnalysis;

namespace Cornerstone.DependencyInjection.SourceGenerator;
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

}
