-- ShipMethod

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ShipMethod' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[ShipMethod]
    (
        [ShipMethodID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ShipBase] MONEY NOT NULL DEFAULT (0.00),
        [ShipRate] MONEY NOT NULL DEFAULT (0.00),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
