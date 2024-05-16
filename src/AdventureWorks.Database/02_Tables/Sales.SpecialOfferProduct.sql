-- SpecialOfferProduct

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SpecialOfferProduct' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SpecialOfferProduct]
    (
        [SpecialOfferID] INT NOT NULL,
        [ProductID] INT NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
