using System.Collections.Immutable;
using System.Text;
using Temelie.Database.Models;
using Microsoft.CodeAnalysis;

namespace Temelie.Entities.SourceGenerator;

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
        var sbExports = new StringBuilder();

        void addTable(TableModel table)
        {
            var className = table.ClassName;

            sbImplements.Append($@",
                                     IRepositoryContext<{className}>");
            sbExports.AppendLine($"[ExportTransient(typeof(IRepositoryContext<{className}>))]");

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

                if (column.ColumnName != column.PropertyName)
                {
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .HasColumnName(\"{column.ColumnName}\");");
                }

                if (columnProperties.Length > 0)
                {
                    props.Append($@"
            builder.Property(p => p.{column.PropertyName}){columnProperties};");
                }
            }

            var config = new StringBuilder();

            if (keys.Count > 1)
            {
                config.AppendLine($"            builder.HasKey(i => new {{ {string.Join(", ", keys.Select(i => $"i.{i}"))} }});");
            }
            else if (keys.Count == 1)
            {
                config.AppendLine($"            builder.HasKey(i => i.{keys[0]});");

            }
            else
            {
                config.AppendLine($"            builder.HasNoKey();");
            }

            if (table.IsView)
            {
                config.AppendLine($"            builder.ToView(\"{table.TableName}\");");
            }
            else
            {
                config.AppendLine($"            builder.ToTable(\"{table.TableName}\");");
            }

            sbModelBuilder.AppendLine($@"
        modelBuilder.Entity<{className}>(builder =>
        {{
{config}
{props}
        }});
");
        }

        foreach (var table in databaseModel.Tables)
        {
            addTable(table);
        }

        foreach (var table in databaseModel.Views)
        {
            addTable(table);
        }

        var sb = new StringBuilder();

        sb.AppendLine($@"using AdventureWorks.Entities;
using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Repository;
using Temelie.Repository.EntityFrameworkCore;

namespace {ns};

{sbExports}public abstract partial class BaseDbContext : DbContext{sbImplements}
{{

    private readonly IServiceProvider _serviceProvider;

    public BaseDbContext(IServiceProvider serviceProvider, DbContextOptions options) : base(options)
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
