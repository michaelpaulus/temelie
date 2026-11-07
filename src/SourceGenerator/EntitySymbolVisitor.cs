using Microsoft.CodeAnalysis;

namespace Temelie.Repository.SourceGenerator;

public class EntitySymbolVisitor : SymbolVisitor
{

    public EntitySymbolVisitor(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    private readonly IList<Entity> _entities = new List<Entity>();
    private readonly CancellationToken _cancellationToken;

    public IEnumerable<Entity> Entities => _entities;

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
        _cancellationToken.ThrowIfCancellationRequested();
        if (!symbol.IsAbstract && symbol.AllInterfaces.Any(i => i.FullName().StartsWith("Temelie.Entities.IEntity<")))
        {
            var props = new List<EntityProperty>();

            var tableName = symbol.Name;
            var schema = "";
            var isView = true;
            var hasTrgger = false;

            foreach (var attr in symbol.GetAttributes())
            {
                var attributeName = attr.AttributeClass?.FullName();
                if (attributeName == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")
                {
                    tableName = attr.ConstructorArguments[0].Value as string ?? tableName;
                    foreach (var na in attr.NamedArguments)
                    {
                        if (na.Key == "Schema")
                        {
                            schema = na.Value.Value as string ?? schema;
                        }
                    }
                }
                if (attributeName == "Temelie.Entities.HasTriggerAttribute")
                {
                    hasTrgger = true;
                }
            }

            foreach (var prop in symbol.GetMembers())
            {
                if (prop is IPropertySymbol propSymbol)
                {
                    var order = props.Count + 1;
                    var isPrimaryKey = false;
                    var isIdentity = false;
                    var isComputed = false;
                    var columnName = propSymbol.Name;
                    var isNullable = false;
                    int? precision = null;
                    int? scale = null;

                    foreach (var attr in propSymbol.GetAttributes())
                    {
                        var attributeName = attr.AttributeClass?.FullName();
                        if (attributeName == "System.ComponentModel.DataAnnotations.KeyAttribute")
                        {
                            isPrimaryKey = true;
                            isView = false;
                        }
                        if (attributeName == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 1)
                            {
                                columnName = attr.ConstructorArguments[0].Value as string ?? columnName;
                            }
                            foreach (var arg in attr.NamedArguments)
                            {
                                if (arg.Key == "Order" && arg.Value.Value is int columnOrder)
                                {
                                    order = columnOrder;
                                }
                            }
                        }
                        if (attributeName == "System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 1)
                            {
                                var databaseGeneratedOption = attr.ConstructorArguments[0].Value?.ToString();
                                isIdentity = databaseGeneratedOption == "1";
                                isComputed = databaseGeneratedOption == "2";
                            }
                            else
                            {

                            }
                        }
                        if (attributeName == "Temelie.Entities.ColumnPrecisionAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 2 &&
                                attr.ConstructorArguments[0].Value is int columnPrecision &&
                                attr.ConstructorArguments[1].Value is int columnScale)
                            {
                                precision = columnPrecision;
                                scale = columnScale;
                            }
                        }
                        isNullable = propSymbol.Type.IsNullable();
                    }

                    var columnType = propSymbol.Type;

                    var propType = "int";

                    if (columnType is INamedTypeSymbol namedTypeSymbol)
                    {
                        if (namedTypeSymbol.IsNullable() &&
                            namedTypeSymbol.TryGetNonNullable(out var nonNullableType) &&
                            nonNullableType is INamedTypeSymbol nonNullableNamedType)
                        {
                            namedTypeSymbol = nonNullableNamedType;
                        }

                        foreach (var member in namedTypeSymbol.GetMembers())
                        {
                            if (member.Name == "Value" && member is IPropertySymbol propertySymbol)
                            {
                                propType = propertySymbol.Type.FullName();
                            }
                        }
                    }

                    props.Add(new EntityProperty(columnType.FullName(), propType, prop.Name, order, columnName, precision, scale, isPrimaryKey, isIdentity, isComputed, isNullable));
                }
            }

            if (props.Count > 0)
            {
                _entities.Add(new Entity(symbol.FullName(), symbol.Name, tableName, schema, isView, hasTrgger, new EquatableArray<EntityProperty>(props.ToArray())));
            }
        }
    }

}
