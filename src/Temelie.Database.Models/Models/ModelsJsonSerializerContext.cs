
using System.Text.Json.Serialization;

namespace Temelie.Database.Models;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
[JsonSerializable(typeof(CheckConstraintModel))]
[JsonSerializable(typeof(ColumnModel))]
[JsonSerializable(typeof(ColumnTypeModel))]
[JsonSerializable(typeof(ConnectionStringModel))]
[JsonSerializable(typeof(DatabaseModel))]
[JsonSerializable(typeof(DatabaseObjectModel))]
[JsonSerializable(typeof(ForeignKeyModel))]
[JsonSerializable(typeof(ForeignKeyDetailModel))]
[JsonSerializable(typeof(IndexColumnModel))]
[JsonSerializable(typeof(IndexModel))]
[JsonSerializable(typeof(Mapping))]
[JsonSerializable(typeof(Model))]
[JsonSerializable(typeof(SecurityPolicyModel))]
[JsonSerializable(typeof(SecurityPolicyPredicate))]
[JsonSerializable(typeof(TableMapping))]
[JsonSerializable(typeof(TableModel))]
[JsonSerializable(typeof(TriggerModel))]
[JsonSerializable(typeof(ExtendedProperty))]
[JsonSerializable(typeof(IEnumerable<ExtendedProperty>))]
internal partial class ModelsJsonSerializerContext : JsonSerializerContext
{

}
