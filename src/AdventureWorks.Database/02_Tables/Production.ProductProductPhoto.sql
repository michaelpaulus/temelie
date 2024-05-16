-- ProductProductPhoto

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductProductPhoto' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductProductPhoto]
    (
        [ProductID] INT NOT NULL,
        [ProductPhotoID] INT NOT NULL,
        [Primary] FLAG NOT NULL DEFAULT (0),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
