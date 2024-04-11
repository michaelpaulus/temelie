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
            indexes.name = 'PK_ProductCategory_ProductCategoryID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductCategory] ADD CONSTRAINT [PK_ProductCategory_ProductCategoryID] PRIMARY KEY CLUSTERED
    (
    [ProductCategoryID]
    )
GO
