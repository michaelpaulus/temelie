using System.Text;
using Microsoft.CodeAnalysis;
using Temelie.Repository.SourceGenerator;

namespace Temelie.Entities.SourceGenerator;

[Generator]
public class DbContextIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select(static (c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new EntitySymbolVisitor(token);
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.Entities;
        });

        context.RegisterSourceOutput(options.Combine(compliationProvider).WithComparer(new EntityListEqualityComparer()), Generate);
    }

    static void Generate(SourceProductionContext context, (string RootNamespace, IEnumerable<Entity> Entities) result)
    {
        var generated = Generate(result.RootNamespace, result.Entities);
        foreach (var item in generated)
        {
            context.AddSource(item.Name, item.Code);
        }
    }

    public static IEnumerable<(string Name, string Code)> Generate(string rootNamespace, IEnumerable<Entity> entities)
    {
        var ns = rootNamespace;

        var list = new List<(string Name, string Codde)>();

        void addEntity(Entity entity)
        {
            var sbModelBuilder = new StringBuilder();
            var sbRepositoryContext = new StringBuilder();
            var sbImplements = new StringBuilder();
            var sbExports = new StringBuilder();

            var className = entity.FullType.Split('.').Last();

            sbImplements.Append($@"IRepositoryContext<{entity.FullType}>");

            sbExports.AppendLine($"[ExportTransient(typeof(IRepositoryContext<{entity.FullType}>))]");

            sbRepositoryContext.AppendLine($@"    public DbSet<{entity.FullType}> {className} {{ get; set; }}
    DbContext IRepositoryContext<{entity.FullType}>.DbContext => this;
    DbSet<{entity.FullType}> IRepositoryContext<{entity.FullType}>.DbSet => {className};");

            var props = new StringBuilder();
            var keys = new List<string>();

            foreach (var column in entity.Properties.OrderBy(i => i.Order))
            {
                var columnProperties = new StringBuilder();

                if (column.IsPrimaryKey)
                {
                    keys.Add(column.Name);
                    if (!column.IsIdentity)
                    {
                        columnProperties.AppendLine();
                        columnProperties.Append($"                .ValueGeneratedNever()");
                    }

                }

                if (column.IsIdentity)
                {
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .UseIdentityColumn(_serviceProvider)");
                }

                if (column.ColumnName != column.Name)
                {
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .HasColumnName(\"{column.ColumnName}\")");
                }

                if (column.Precision.HasValue && column.Scale.HasValue)
                {
                    columnProperties.AppendLine();
                    columnProperties.Append($"                .HasPrecision({column.Precision.Value}, {column.Scale.Value})");
                }

                if (columnProperties.Length > 0)
                {
                    props.Append($@"            builder.Property(p => p.{column.Name}){columnProperties};");
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

            if (entity.IsView)
            {
                config.AppendLine($"            builder.ToView(\"{entity.TableName}\", \"{entity.Schema}\");");
            }
            else
            {
                config.AppendLine($"            builder.ToTable(\"{entity.TableName}\", \"{entity.Schema}\");");
            }

            sbModelBuilder.AppendLine($@"
        modelBuilder.Entity<{entity.FullType}>(builder =>
        {{
{config}{props}
        }});");

            var sb = new StringBuilder();

            sb.AppendLine($@"using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Repository.EntityFrameworkCore;

namespace {ns};

{sbExports}public abstract partial class DbContextBase : {sbImplements}
{{
{sbRepositoryContext}
}}

[ExportProvider(typeof(IModelBuilderProvider))]
public partial class {entity.FullType.Replace(".", "_")}ModelBuilderProvider : IModelBuilderProvider
{{
    private readonly IServiceProvider _serviceProvider;

    public {entity.FullType.Replace(".", "_")}ModelBuilderProvider(IServiceProvider serviceProvider)
    {{
        _serviceProvider = serviceProvider;
    }}

    public void OnModelCreating(ModelBuilder modelBuilder)
    {{
{sbModelBuilder}
    }}
}}");

            list.Add(($"{ns}.{entity.FullType}.DbContextBase.g", sb.ToString()));
        }

        foreach (var entity in entities)
        {
            addEntity(entity);
        }

        void addBase()
        {
            var sb = new StringBuilder();

            sb.AppendLine($@"using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Temelie.Repository.EntityFrameworkCore;

namespace {ns};

public abstract partial class DbContextBase : DbContext
{{

    private readonly IServiceProvider _serviceProvider;

    public DbContextBase(IServiceProvider serviceProvider, DbContextOptions options) : base(options)
    {{
        _serviceProvider = serviceProvider;
    }}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {{
        base.OnModelCreating(modelBuilder);
        foreach (var provider in _serviceProvider.GetServices<IModelBuilderProvider>())
        {{
            provider.OnModelCreating(modelBuilder);
        }}
    }}
}}");
            list.Add(($"{ns}.DbContextBase.g", sb.ToString()));
        }

        addBase();

        return list.ToArray();
    }

}
