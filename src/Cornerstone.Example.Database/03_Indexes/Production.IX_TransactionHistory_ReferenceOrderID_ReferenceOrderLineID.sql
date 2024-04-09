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
            indexes.name = 'IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID' AND
            schemas.name = 'Production'
    )
    CREATE NONCLUSTERED INDEX [IX_TransactionHistory_ReferenceOrderID_ReferenceOrderLineID] ON [Production].[TransactionHistory]
    (
    [ReferenceOrderID],
    [ReferenceOrderLineID]
    )
GO
