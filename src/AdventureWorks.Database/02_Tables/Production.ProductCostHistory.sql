-- ProductCostHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductCostHistory' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductCostHistory]
    (
        [ProductID] INT NOT NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NULL,
        [StandardCost] MONEY NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
