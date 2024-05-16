-- UnitMeasure

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'UnitMeasure' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[UnitMeasure]
    (
        [UnitMeasureCode] NCHAR(3) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
