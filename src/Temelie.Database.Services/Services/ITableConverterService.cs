using System.Data;
using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Services;
public interface ITableConverterService
{
    void ConvertBulk(IProgress<TableProgress> progress, TableModel sourceTable, ConnectionStringModel sourceConnectionString, TableModel targetTable, ConnectionStringModel targetConnectionString, bool trimStrings, int batchSize, bool useTransaction = true);
    void ConvertBulk(IProgress<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, ConnectionStringModel targetConnectionString, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true);
    void ConvertBulk(IProgress<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, DbConnection targetConnection, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true);
    void ConvertTable(TableConverterSettings settings, TableModel sourceTable, IProgress<TableProgress> progress);
    void ConvertTable(TableConverterSettings settings, TableModel sourceTable, TableModel targetTable, IProgress<TableProgress> progress, bool throwOnFailure = false);
    void ConvertTables(TableConverterSettings settings, IProgress<TableProgress> progress, int maxDegreeOfParallelism);
    IEnumerable<ColumnModel> GetMatchedColumns(IEnumerable<ColumnModel> sourceColumns, IEnumerable<ColumnModel> targetColumns);
    int GetRowCount(TableModel table, DbConnection connection);
}
