using System.Collections.Immutable;
using System.Text;
using Cornerstone.Database.Models;
using Microsoft.CodeAnalysis;

namespace Cornerstone.Entities.SourceGenerator;

[Generator]
public class DbContextIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var assemblyNames = context.CompilationProvider.Select((c, _) => c.AssemblyName);

        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".sql.json"))
            .Select((a, c) => (a.Path, a.GetText(c)!.ToString()))
            .Collect();

        var result = assemblyNames.Combine(files);

        context.RegisterSourceOutput(result, Generate);
    }

    void Generate(SourceProductionContext context, (string assemblyName, ImmutableArray<(string FileName, string Text)> files) result)
    {
        var assemblyName = result.assemblyName;
        var ns = assemblyName;
        var databaseModel = DatabaseModel.CreateFromFiles(result.files);

        var pkColumns = new List<ColumnModel>();

        var sbModelBuilder = new StringBuilder();
        var sbRepositoryContext = new StringBuilder();
        var sbImplements = new StringBuilder();

        foreach (var table in databaseModel.Tables)
        {
            var className = table.TableName;

            var pk = databaseModel.PrimaryKeys.FirstOrDefault(i => i.TableName == table.TableName && i.SchemaName == table.SchemaName);

            sbImplements.Append($", IRepositoryContext<{className}>");

            sbRepositoryContext.AppendLine($@"
    public DbSet<{className}> {className} {{ get; set; }}
    DbContext IRepositoryContext<{className}>.DbContext => this;
    DbSet<{className}> IRepositoryContext<{className}>.DbSet => {className};
");
            if (pk is not null)
            {
                var sbKey = new StringBuilder();

                if (pk.Columns.Count > 0)
                {
                    sbKey.Append($"new {{ {string.Join(", ", pk.Columns.Select(i => $"i.{NormalizeColumnName(i.ColumnName)}"))} }}");
                }
                else
                {
                    sbKey.Append($"i.{NormalizeColumnName(pk.Columns[0].ColumnName)}");
                }
                var props = new StringBuilder();

                foreach (var key in pk.Columns)
                {
                    var name = NormalizeColumnName(key.ColumnName);
                    props.Append($@"
            builder.Property(p => p.{name})
                .HasConversion(id => id.Value, value => new {name}(value));
");
                }

                sbModelBuilder.AppendLine($@"
        modelBuilder.Entity<{className}>(builder =>
        {{
            builder.HasKey(i => {sbKey});
{props}
        }});
");
            }
        }

        var sb = new StringBuilder();

        sb.AppendLine($@"
using AdventureWorks.Entities;
using Cornerstone.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace {ns};

public partial class BaseDbContext : DbContext{sbImplements}
{{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
        {sbModelBuilder}
    }}

    {sbRepositoryContext}
}}
");

        context.AddSource($"{ns}.BaseDbContext.g", sb.ToString());
    }

    private string NormalizeColumnName(string columnName)
    {
        columnName = columnName.Replace(" ", "");

        if (columnName.Contains("ID"))
        {
            var index = columnName.IndexOf("ID");
            if (index > 0)
            {
                var previous = columnName.Substring(index - 1, 1);
                if (previous.Equals(previous.ToLower()))
                {
                    var first = columnName.Substring(0, index);
                    var last = "";
                    if (index > columnName.Length)
                    {
                        last = columnName.Substring(index + 3);
                    }
                    columnName = $"{first}Id{last}";

                }
            }
        }

        return columnName;
    }

}
