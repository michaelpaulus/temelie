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
            indexes.name = 'PK_ProductInventory_ProductID_LocationID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductInventory] ADD CONSTRAINT [PK_ProductInventory_ProductID_LocationID] PRIMARY KEY CLUSTERED
    (
    [ProductID],
    [LocationID]
    )
GO
