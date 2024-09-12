using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace AdventureWorks.Database;
[ExportTransient(typeof(IDatabaseModelProvider))]
public class DatabaseModelProvider : IDatabaseModelProvider
{

    public void Initialize(DefinitionModel model)
    {
        model.Definition = FormatSql(model.Definition);
    }

    public void Initialize(TriggerModel model)
    {
        model.Definition = FormatSql(model.Definition);
    }

    private string FormatSql(string sql)
    {
        var parser = new Microsoft.SqlServer.TransactSql.ScriptDom.TSql160Parser(false);
        using var reader = new System.IO.StringReader(sql);

        var results = parser.Parse(reader, out var errors);

        var scriptGenerator = new Microsoft.SqlServer.TransactSql.ScriptDom.Sql160ScriptGenerator(new Microsoft.SqlServer.TransactSql.ScriptDom.SqlScriptGeneratorOptions()
        {
            AlignClauseBodies = true,
            AlignColumnDefinitionFields = false,
            AlignSetClauseItem = false,
            AllowExternalLanguagePaths = false,
            AllowExternalLibraryPaths = false,
            AsKeywordOnOwnLine = false,
            NewLineAfterBinaryBooleanExpresson = true,
            NewLineAfterFromClause = true,
            NewLineAfterGroupByKeyword = true,
            NewLineAfterHavingKeyword = true,
            NewLineAfterOnKeyword = true,
            NewLineAfterOrderByKeyword = true,
            NewLineAfterSelectKeyword = true,
            NewLineAfterWhereKeyword = true,
            NewLineBeforeBinaryBooleanExpresson = false,
            NewLineBeforeCloseParenthesisInMultilineList = true,
            NewLineBeforeFromClause = true,
            NewLineBeforeGroupByClause = false,
            NewLineBeforeHavingClause = false,
            NewLineBeforeJoinClause = false,
            NewLineBeforeOffsetClause = false,
            NewLineBeforeOnKeyword = false,
            NewLineBeforeOpenParenthesisInMultilineList = true,
            NewLineBeforeOrderByClause = false,
            NewLineBeforeOutputClause = false,
            NewLineBeforeWhereClause = false,
            NewLineBeforeWindowClause = false
        });

        scriptGenerator.GenerateScript(results, out var output);

        return output.Replace("\r\n", "\n").Replace("\r", "\n").Trim('\n');
    }

}
