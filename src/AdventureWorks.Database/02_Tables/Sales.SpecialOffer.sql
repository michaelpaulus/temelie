-- SpecialOffer

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SpecialOffer' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SpecialOffer]
    (
        [SpecialOfferID] INT IDENTITY (1, 1) NOT NULL,
        [Description] NVARCHAR(255) NOT NULL,
        [DiscountPct] SMALLMONEY NOT NULL DEFAULT (0.00),
        [Type] NVARCHAR(50) NOT NULL,
        [Category] NVARCHAR(50) NOT NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NOT NULL,
        [MinQty] INT NOT NULL DEFAULT (0),
        [MaxQty] INT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
