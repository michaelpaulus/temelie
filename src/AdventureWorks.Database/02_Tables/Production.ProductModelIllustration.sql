-- ProductModelIllustration

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductModelIllustration' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductModelIllustration]
    (
        [ProductModelID] INT NOT NULL,
        [IllustrationID] INT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
