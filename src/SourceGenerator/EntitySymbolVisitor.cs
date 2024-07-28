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
            var schema = "dbo";
            var isView = true;

            foreach (var attr in symbol.GetAttributes())
            {
                if (attr.AttributeClass.FullName() == "System.ComponentModel.DataAnnotations.Schema.TableAttribute")
                {
                    tableName = (string)attr.ConstructorArguments[0].Value;
                    foreach (var na in attr.NamedArguments)
                    {
                        if (na.Key == "Schema")
                        {
                            schema = (string)na.Value.Value;
                        }
                    }
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
                        if (attr.AttributeClass.FullName() == "System.ComponentModel.DataAnnotations.KeyAttribute")
                        {
                            isPrimaryKey = true;
                            isView = false;
                        }
                        if (attr.AttributeClass.FullName() == "System.ComponentModel.DataAnnotations.Schema.ColumnAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 1)
                            {
                                columnName = (string)attr.ConstructorArguments[0].Value;
                            }
                            foreach (var arg in attr.NamedArguments)
                            {
                                if (arg.Key == "Order")
                                {
                                    order = (int)arg.Value.Value;
                                }
                            }
                        }
                        if (attr.AttributeClass.FullName() == "System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 1)
                            {
                                isIdentity = attr.ConstructorArguments[0].Value.ToString().Equals("1");
                                isComputed = attr.ConstructorArguments[0].Value.ToString().Equals("2");
                            }
                            else
                            {
                                
                            }
                        }
                        if (attr.AttributeClass.FullName() == "Temelie.Entities.ColumnPrecisionAttribute")
                        {
                            if (attr.ConstructorArguments.Length == 2)
                            {
                                precision = (int)attr.ConstructorArguments[0].Value;
                                scale = (int)attr.ConstructorArguments[1].Value;
                            }
                        }
                        isNullable = propSymbol.Type.IsNullable();

                        var propertyType = propSymbol.Type;

                        if (isNullable)
                        {
                            propertyType.TryGetNonNullable(out propertyType);
                        }

                    }

                    var columnType = propSymbol.Type;

                    var propType = "int";

                    if (columnType is INamedTypeSymbol namedTypeSymbol)
                    {
                        if (namedTypeSymbol.IsNullable())
                        {
                            namedTypeSymbol.TryGetNonNullable(out var temp);
                            namedTypeSymbol = temp as INamedTypeSymbol;
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
                _entities.Add(new Entity(symbol.FullName(), symbol.Name, tableName, schema, isView, new EquatableArray<EntityProperty>(props.ToArray())));
            }
        }
    }

}
