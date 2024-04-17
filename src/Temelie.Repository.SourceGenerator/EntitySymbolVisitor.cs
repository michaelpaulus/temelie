using Microsoft.CodeAnalysis;
using Temelie.Entities.SourceGenerator;

namespace Temelie.Repository.SourceGenerator;

public class EntitySymbolVisitor : SymbolVisitor
{

    public EntitySymbolVisitor()
    {
    }

    private readonly IList<Entity> _entities = new List<Entity>();
    public IEnumerable<Entity> Entities => _entities;

    public override void VisitNamespace(INamespaceSymbol symbol)
    {
        foreach (var member in symbol.GetMembers())
        {
            member.Accept(this);
        }
    }

    public override void VisitNamedType(INamedTypeSymbol symbol)
    {
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
                    var isEntityId = false;
                    var isNullable = false;

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
                                isIdentity = attr.ConstructorArguments[0].Value.ToString().Contains("Identity");
                                isComputed = attr.ConstructorArguments[0].Value.ToString().Contains("Computed");
                            }
                        }
                        if (attr.AttributeClass.Name == "NullableAttribute")
                        {
                            isNullable = true;
                        }
                        if (attr.AttributeClass.Name == "EntityIdAttribute")
                        {
                            isEntityId = true;
                        }
                    }

                    var propType = propSymbol.Type;

                    props.Add(new EntityProperty(propType.FullName(), prop.Name, order, columnName, isPrimaryKey, isIdentity, isComputed, isEntityId, isNullable));
                }
            }

            if (props.Count > 0)
            {
                _entities.Add(new Entity(symbol.FullName(), symbol.Name, tableName, schema, isView, props.ToArray()));
            }
        }
    }

}
