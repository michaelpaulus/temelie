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
            indexes.name = 'PK_ProductVendor_ProductID_BusinessEntityID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[ProductVendor] ADD CONSTRAINT [PK_ProductVendor_ProductID_BusinessEntityID] PRIMARY KEY CLUSTERED
    (
    [ProductID],
    [BusinessEntityID]
    )
GO
