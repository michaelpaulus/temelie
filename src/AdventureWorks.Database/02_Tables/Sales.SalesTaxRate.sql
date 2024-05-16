-- SalesTaxRate

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesTaxRate' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesTaxRate]
    (
        [SalesTaxRateID] INT IDENTITY (1, 1) NOT NULL,
        [StateProvinceID] INT NOT NULL,
        [TaxType] TINYINT NOT NULL,
        [TaxRate] SMALLMONEY NOT NULL DEFAULT (0.00),
        [Name] NAME NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
