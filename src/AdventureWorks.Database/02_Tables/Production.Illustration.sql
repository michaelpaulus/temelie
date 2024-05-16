-- Illustration

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Illustration' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Illustration]
    (
        [IllustrationID] INT IDENTITY (1, 1) NOT NULL,
        [Diagram] XML NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
