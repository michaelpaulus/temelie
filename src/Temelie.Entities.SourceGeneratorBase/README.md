# Temelie.Entities.SourceGeneratorBase

The building blocks of [Temelie.Entities.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities.SourceGenerator/README.md), for writing your own entity generator.

```shell
dotnet add package Temelie.Entities.SourceGeneratorBase
```

`EntityIncrementalGeneratorBase` implements `IIncrementalGenerator`. Derive from it in a `netstandard2.0` analyzer project and override the virtual members you want to change:

| Member | Controls |
| --- | --- |
| `GetColumnProperties` | Which columns become properties, and their types and defaults. |
| `GetRecordAttributes`, `GetRecordProperty`, `GetRecordExtends` | The generated record. |
| `GetInterfaceExtends`, `GetInterfaceProperty` | The generated interface (including audit interfaces). |
| `Generate(rootNamespace, files, partialProperties)` | All generated files. |

```csharp
[Generator]
public class MyEntityGenerator : EntityIncrementalGeneratorBase
{
    protected override string GetRecordExtends(TableModel table, IEnumerable<ColumnProperty> props) =>
        base.GetRecordExtends(table, props) + ", IMyMarker";
}
```

Like any analyzer, your generator package must ship this assembly and its `System.Text.Json` dependencies alongside it in `analyzers/dotnet/cs`. See the `Temelie.Entities.SourceGenerator` project file for an example.
