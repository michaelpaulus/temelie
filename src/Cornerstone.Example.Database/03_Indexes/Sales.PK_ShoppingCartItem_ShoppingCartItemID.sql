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
            indexes.name = 'PK_ShoppingCartItem_ShoppingCartItemID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[ShoppingCartItem] ADD CONSTRAINT [PK_ShoppingCartItem_ShoppingCartItemID] PRIMARY KEY CLUSTERED
    (
    [ShoppingCartItemID]
    )
GO
