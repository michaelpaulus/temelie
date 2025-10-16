using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Temelie.Database.Models;
using Temelie.Database.Services;

namespace Temelie.Database.Providers.Mssql;
public partial class DatabaseProvider
{

    public override void ConvertBulk(TableConverterService service, IProgress<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, DbConnection targetConnection, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true)
    {
        progress?.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        int intProgress = 0;

        if (validateTargetTable)
        {
            var targetRowCount = service.GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        void bulkCopy(SqlTransaction? transaction1)
        {
            int intRowIndex = 0;

            var sourceMatchedColumns = service.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
            var targetMatchedColumns = service.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

            using (SqlBulkCopy bcp = new SqlBulkCopy((SqlConnection)targetConnection, SqlBulkCopyOptions.KeepIdentity, transaction1))
            {
                bcp.DestinationTableName = $"[{targetTable.SchemaName}].[{targetTable.TableName}]";
                bcp.BatchSize = batchSize == 0 ? 10000 : batchSize;
                bcp.BulkCopyTimeout = 600;
                bcp.NotifyAfter = bcp.BatchSize;

                foreach (var targetColumn in targetMatchedColumns)
                {
                    bcp.ColumnMappings.Add(targetColumn.ColumnName, targetColumn.ColumnName);
                }

                bcp.SqlRowsCopied += (object sender, SqlRowsCopiedEventArgs e) =>
                {
                    if (progress != null &&
                        sourceRowCount > 0)
                    {
                        intRowIndex += bcp.BatchSize;

                        if (intRowIndex > sourceRowCount)
                        {
                            intRowIndex = sourceRowCount;
                        }

                        int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)sourceRowCount * 100);

                        if (intProgress != intNewProgress &&
                            intNewProgress < 100)
                        {
                            intProgress = intNewProgress;
                            progress.Report(new TableProgress() { ProgressPercentage = intProgress, Table = sourceTable });
                        }
                    }
                };

                bcp.WriteToServer(sourceReader);

            }

        }

        if (System.Transactions.Transaction.Current == null && useTransaction)
        {
            using (var transaction = (SqlTransaction)targetConnection.BeginTransaction())
            {
                bulkCopy(transaction);
                transaction.Commit();
            }
        }
        else
        {
            bulkCopy(null);
        }

        if (progress != null &&
            intProgress != 100)
        {
            progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
        }
    }

}
