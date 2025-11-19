using System.Data;
using System.Data.Common;
using Temelie.Database.Models;
using Temelie.Database.Services;
using MySqlConnector;

namespace Temelie.Database.Providers.MySql;
public partial class DatabaseProvider
{

    public override void ConvertBulk(TableConverterService service, Action<TableProgress> progress, TableModel sourceTable, IDataReader sourceReader, int sourceRowCount, TableModel targetTable, DbConnection targetConnection, bool trimStrings, int batchSize, bool useTransaction = true, bool validateTargetTable = true)
    {
        progress?.Invoke(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        int intProgress = 0;

        if (validateTargetTable)
        {
            var targetRowCount = service.GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Invoke(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        void bulkCopy(MySqlTransaction transaction1)
        {
            int intRowIndex = 0;

            var sourceMatchedColumns = service.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
            var targetMatchedColumns = service.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

            var bcp = new MySqlBulkCopy((MySqlConnection)targetConnection, transaction1);

            batchSize = batchSize == 0 ? 10000 : batchSize;

            bcp.DestinationTableName = $"`{targetTable.TableName}`";
            bcp.BulkCopyTimeout = 600;
            bcp.NotifyAfter = batchSize;

            foreach (var targetColumn in targetMatchedColumns)
            {
                var sourceIndex = sourceTable.Columns.IndexOf(sourceTable.Columns.First(i => i.ColumnName.Equals(targetColumn.ColumnName, StringComparison.OrdinalIgnoreCase)));

                bcp.ColumnMappings.Add(new MySqlBulkCopyColumnMapping(sourceIndex, targetColumn.ColumnName));
            }

            bcp.MySqlRowsCopied += (object sender, MySqlRowsCopiedEventArgs e) =>
            {
                if (progress != null &&
                    sourceRowCount > 0)
                {
                    intRowIndex += batchSize;

                    if (intRowIndex > sourceRowCount)
                    {
                        intRowIndex = sourceRowCount;
                    }

                    int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)sourceRowCount * 100);

                    if (intProgress != intNewProgress &&
                        intNewProgress < 100)
                    {
                        intProgress = intNewProgress;
                        progress.Invoke(new TableProgress() { ProgressPercentage = intProgress, Table = sourceTable });
                    }
                }
            };

            bcp.WriteToServer(sourceReader);
        }

        if (System.Transactions.Transaction.Current == null && useTransaction)
        {
            using (var transaction = (MySqlTransaction)targetConnection.BeginTransaction())
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
            progress.Invoke(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
        }
    }

}
