-- Culture

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Culture' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Culture]
    (
        [CultureID] NCHAR(6) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
