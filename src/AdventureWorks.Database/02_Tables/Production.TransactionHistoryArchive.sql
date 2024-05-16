-- TransactionHistoryArchive

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'TransactionHistoryArchive' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[TransactionHistoryArchive]
    (
        [TransactionID] INT NOT NULL,
        [ProductID] INT NOT NULL,
        [ReferenceOrderID] INT NOT NULL,
        [ReferenceOrderLineID] INT NOT NULL DEFAULT (0),
        [TransactionDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [TransactionType] NCHAR(1) NOT NULL,
        [Quantity] INT NOT NULL,
        [ActualCost] MONEY NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
