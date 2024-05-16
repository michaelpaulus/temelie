-- ProductInventory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductInventory' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductInventory]
    (
        [ProductID] INT NOT NULL,
        [LocationID] SMALLINT NOT NULL,
        [Shelf] NVARCHAR(10) NOT NULL,
        [Bin] TINYINT NOT NULL,
        [Quantity] SMALLINT NOT NULL DEFAULT (0),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
