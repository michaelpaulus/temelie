-- ProductDescription

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductDescription' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductDescription]
    (
        [ProductDescriptionID] INT IDENTITY (1, 1) NOT NULL,
        [Description] NVARCHAR(400) NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
