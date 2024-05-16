-- Shift

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Shift' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[Shift]
    (
        [ShiftID] TINYINT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [StartTime] TIME(7) NOT NULL,
        [EndTime] TIME(7) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
