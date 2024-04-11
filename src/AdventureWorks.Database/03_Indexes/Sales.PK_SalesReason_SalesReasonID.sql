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
            indexes.name = 'PK_SalesReason_SalesReasonID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesReason] ADD CONSTRAINT [PK_SalesReason_SalesReasonID] PRIMARY KEY CLUSTERED
    (
    [SalesReasonID]
    )
GO
