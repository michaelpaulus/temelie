IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'PK_TransactionHistoryArchive_TransactionID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[TransactionHistoryArchive] ADD CONSTRAINT [PK_TransactionHistoryArchive_TransactionID] PRIMARY KEY CLUSTERED
    (
    [TransactionID]
    )
GO
