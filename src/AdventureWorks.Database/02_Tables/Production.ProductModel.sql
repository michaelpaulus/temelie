-- ProductModel

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductModel' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductModel]
    (
        [ProductModelID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [CatalogDescription] XML NULL,
        [Instructions] XML NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
