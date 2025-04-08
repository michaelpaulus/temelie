-- Migrations

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Migrations' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[Migrations]
    (
        [Id] NVARCHAR(500) NOT NULL,
        [Date] DATETIME NOT NULL,
        [Skipped] BIT NULL
    )
GO
