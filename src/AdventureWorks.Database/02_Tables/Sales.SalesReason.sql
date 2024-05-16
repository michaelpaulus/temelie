-- SalesReason

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesReason' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesReason]
    (
        [SalesReasonID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ReasonType] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
