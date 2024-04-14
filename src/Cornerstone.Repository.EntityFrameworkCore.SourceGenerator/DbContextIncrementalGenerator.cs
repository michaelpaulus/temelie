using System.Collections.Immutable;
using System.Text;
using System.Xml.Linq;
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

        context.RegisterImplementationSourceOutput(result, Generate);
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

            sbImplements.Append($", IRepositoryContext<{className}>");

            sbRepositoryContext.AppendLine($@"
    public DbSet<{className}> {className} {{ get; set; }}
    DbContext IRepositoryContext<{className}>.DbContext => this;
    DbSet<{className}> IRepositoryContext<{className}>.DbSet => {className};
");

            var keys = new List<string>();
            var props = new StringBuilder();

            foreach (var column in table.Columns)
            {
                var columnProperties = new StringBuilder();
                if (column.IsPrimaryKey)
                {
                    keys.Add(column.PropertyName);
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .HasConversion(id => id.Value, value => new {column.PropertyName}(value))");
                }

                if (column.IsIdentity)
                {
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .UseIdentityColumn(_serviceProvider)");
                }

                if (columnProperties.Length > 0)
                {
                    props.Append($@"            builder.Property(p => p.{column.PropertyName}){columnProperties};");
                }
            }

            var sbKey = new StringBuilder();

            if (keys.Count > 0)
            {
                sbKey.Append($"            builder.HasKey(i => new {{ {string.Join(", ", keys.Select(i => $"i.{i}"))} }});");
            }
            else
            {
                sbKey.Append($"            builder.HasKey(i => i.{keys[0]});");

            }

            sbModelBuilder.AppendLine($@"
        modelBuilder.Entity<{className}>(builder =>
        {{
{sbKey}
{props}
        }});
");
        }

        var sb = new StringBuilder();

        sb.AppendLine($@"
using AdventureWorks.Entities;
using Cornerstone.Repository.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace {ns};

public partial class BaseDbContext : DbContext{sbImplements}
{{

    private readonly IServiceProvider _serviceProvider;

    public BaseDbContext(IServiceProvider serviceProvider)
    {{
        _serviceProvider = serviceProvider;
    }}

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

}
