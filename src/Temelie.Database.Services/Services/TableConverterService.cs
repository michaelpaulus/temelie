using System.Data;
using System.Data.Common;
using System.Text;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;
[ExportTransient(typeof(ITableConverterService))]
public class TableConverterService : ITableConverterService
{

    private readonly IDatabaseFactory _databaseFactory;
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IEnumerable<ITableConverterReaderColumnValueProvider> _tableConverterReaderColumnValueProviders;

    public TableConverterService(IDatabaseFactory databaseFactory,
        IDatabaseExecutionService databaseExecutionService,
        IEnumerable<ITableConverterReaderColumnValueProvider> tableConverterReaderColumnValueProviders)
    {
        _databaseFactory = databaseFactory;
        _databaseExecutionService = databaseExecutionService;
        _tableConverterReaderColumnValueProviders = tableConverterReaderColumnValueProviders;
    }

    public void ConvertTables(TableConverterSettings settings,
        IProgress<TableProgress> progress,
        int maxDegreeOfParallelism)
    {

        var tables = settings.SourceTables.OrderBy(i => i.TableName);

        var options = new ParallelOptions();
        options.MaxDegreeOfParallelism = maxDegreeOfParallelism;

        Parallel.ForEach(tables, options, (sourceTable) =>
        {
            this.ConvertTable(settings, sourceTable, progress);
        });
    }

    public void ConvertTable(TableConverterSettings settings, Models.TableModel sourceTable, IProgress<TableProgress> progress)
    {

        var targetTable = (
                   from i in settings.TargetTables
                   where
                   i.SchemaName.Equals(sourceTable.SchemaName, StringComparison.InvariantCultureIgnoreCase) &&
                   i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                   select i).FirstOrDefault();

        if (targetTable == null && (sourceTable.SchemaName == "dbo" || string.IsNullOrEmpty(sourceTable.SchemaName)))
        {
            targetTable = (
                 from i in settings.TargetTables
                 where
                 i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                 select i).FirstOrDefault();
        }

        ConvertTable(settings, sourceTable, targetTable, progress);
    }

    public void ConvertTable(TableConverterSettings settings, Models.TableModel sourceTable, Models.TableModel targetTable, IProgress<TableProgress> progress, bool throwOnFailure = false)
    {
        if (targetTable != null)
        {
            try
            {
                this.ConvertBulk(progress, sourceTable, settings.SourceConnectionString, targetTable, settings.TargetConnectionString, settings.TrimStrings, settings.BatchSize, settings.UseTransaction);
            }
            catch (Exception ex)
            {
                string strException = string.Empty;
                if (!(ex.Message.Equals("ERROR [00000] [QODBC] Error: 3250 - This feature is not enabled or not available in this version of QuickBooks.")))
                {
                    strException = ex.ToString();
                }

                if (progress == null || throwOnFailure)
                {
                    if (ex as TableConverterException == null)
                    {
                        throw;
                    }
                    else
                    {
                        throw new TableConverterException(sourceTable, strException, ex);
                    }
                }
                else
                {
                    progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable, ErrorMessage = strException, Exception = ex });
                }

            }
        }
    }

    public void ConvertBulk(IProgress<TableProgress> progress,
      Models.TableModel sourceTable,
      IDataReader sourceReader,
      int sourceRowCount,
      Models.TableModel targetTable,
      ConnectionStringModel targetConnectionString,
      bool trimStrings,
      int batchSize,
      bool useTransaction = true,
      bool validateTargetTable = true)
    {

        var targetDatabaseProvider = _databaseFactory.GetDatabaseProvider(targetConnectionString);
        using (System.Data.Common.DbConnection targetConnection = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        {
            targetDatabaseProvider.ConvertBulk(this, progress, sourceTable, sourceReader, sourceRowCount, targetTable, targetConnection, trimStrings, batchSize, useTransaction, validateTargetTable);
        }
    }

    public void ConvertBulk(IProgress<TableProgress> progress,
     Models.TableModel sourceTable,
     IDataReader sourceReader,
     int sourceRowCount,
     Models.TableModel targetTable,
     DbConnection targetConnection,
     bool trimStrings,
     int batchSize,
     bool useTransaction = true,
     bool validateTargetTable = true)
    {
        var targetDatabaseProvider = _databaseFactory.GetDatabaseProvider(targetConnection);
        targetDatabaseProvider.ConvertBulk(this, progress, sourceTable, sourceReader, sourceRowCount, targetTable, targetConnection, trimStrings, batchSize, useTransaction, validateTargetTable);
    }

    private DbCommand CreateSourceCommand(Models.TableModel sourceTable, Models.TableModel targetTable, System.Data.Common.DbConnection sourceConnection)
    {
        var sourceDatabaseProvider = _databaseFactory.GetDatabaseProvider(sourceConnection);

        StringBuilder sbColumns = new System.Text.StringBuilder();

        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
        var command = _databaseExecutionService.CreateDbCommand(sourceConnection);
        this.SetReadTimeout(sourceDatabaseProvider, command);
        command.CommandText = sourceDatabaseProvider.GetSelectStatement(sourceTable.SchemaName, sourceTable.TableName, sourceMatchedColumns.Select(i => i.ColumnName).ToArray());
        return command;
    }

    public void ConvertBulk(IProgress<TableProgress> progress,
        Models.TableModel sourceTable,
        ConnectionStringModel sourceConnectionString,
        Models.TableModel targetTable,
        ConnectionStringModel targetConnectionString,
        bool trimStrings,
        int batchSize,
        bool useTransaction = true)
    {
        progress?.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        var sourceDatabaseProvider = _databaseFactory.GetDatabaseProvider(sourceConnectionString);

        using (var targetConnection = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        {
            var targetRowCount = GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        int? intSourceRowCount = null;

        if (progress != null)
        {
            using (System.Data.Common.DbConnection sourceConnection = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
            {
                intSourceRowCount = GetRowCount(sourceTable, sourceConnection);
            }
        }

        if (!intSourceRowCount.HasValue || intSourceRowCount.Value > 0)
        {
            using (System.Data.Common.DbConnection sourceConnection = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
            {
                using (var command = CreateSourceCommand(sourceTable, targetTable, sourceConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
                        var targetMatchedColumns = this.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);
                        var reader2 = new TableConverterReader(sourceDatabaseProvider, reader, _tableConverterReaderColumnValueProviders, sourceMatchedColumns, targetMatchedColumns, trimStrings);
                        ConvertBulk(progress, sourceTable, reader2, intSourceRowCount.GetValueOrDefault(), targetTable, targetConnectionString, trimStrings, batchSize, useTransaction, false);
                    }
                }
            }
        }
    }

    public IEnumerable<Models.ColumnModel> GetMatchedColumns(IEnumerable<Models.ColumnModel> sourceColumns, IEnumerable<Models.ColumnModel> targetColumns)
    {
        var list = new List<Models.ColumnModel>();

        foreach (var sourceColumn in sourceColumns)
        {
            var strColumnName = sourceColumn.ColumnName;
            var targetColumn = targetColumns.FirstOrDefault(i => i.ColumnName.Equals(sourceColumn.ColumnName, StringComparison.OrdinalIgnoreCase));
            if (targetColumn != null && !targetColumn.IsComputed)
            {
                list.Add(sourceColumn);
            }
        }

        return list;
    }

    private void SetReadTimeout(IDatabaseProvider databaseProvider, System.Data.Common.DbCommand sourceCommand)
    {
        databaseProvider.SetReadTimeout(sourceCommand);
    }

    public int GetRowCount(Models.TableModel table, System.Data.Common.DbConnection connection)
    {
        return this.GetRowCount(connection, table.SchemaName, table.TableName);
    }

    private int GetRowCount(System.Data.Common.DbConnection connection, string schemaName, string tableName)
    {
        int rowCount = 0;

        var databaseProvider = _databaseFactory.GetDatabaseProvider(connection);

        using (var command = _databaseExecutionService.CreateDbCommand(connection))
        {
            rowCount = databaseProvider.GetRowCount(command, schemaName, tableName);
        }

        return rowCount;
    }

}
