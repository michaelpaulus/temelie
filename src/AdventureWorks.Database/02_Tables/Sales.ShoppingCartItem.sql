-- ShoppingCartItem

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ShoppingCartItem' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[ShoppingCartItem]
    (
        [ShoppingCartItemID] INT IDENTITY (1, 1) NOT NULL,
        [ShoppingCartID] NVARCHAR(50) NOT NULL,
        [Quantity] INT NOT NULL DEFAULT (1),
        [ProductID] INT NOT NULL,
        [DateCreated] DATETIME NOT NULL DEFAULT (GETDATE()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
