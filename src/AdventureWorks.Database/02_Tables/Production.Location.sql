-- Location

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Location' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Location]
    (
        [LocationID] SMALLINT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [CostRate] SMALLMONEY NOT NULL DEFAULT (0.00),
        [Availability] DECIMAL(8, 2) NOT NULL DEFAULT (0.00),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
