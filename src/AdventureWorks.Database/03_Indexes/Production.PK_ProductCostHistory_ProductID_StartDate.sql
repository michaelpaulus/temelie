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
            indexes.name = 'PK_ProductCostHistory_ProductID_StartDate' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductCostHistory] ADD CONSTRAINT [PK_ProductCostHistory_ProductID_StartDate] PRIMARY KEY CLUSTERED
    (
    [ProductID],
    [StartDate]
    )
GO
