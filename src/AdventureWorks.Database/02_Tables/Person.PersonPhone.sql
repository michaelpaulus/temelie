-- PersonPhone

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PersonPhone' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[PersonPhone]
    (
        [BusinessEntityID] INT NOT NULL,
        [PhoneNumber] PHONE NOT NULL,
        [PhoneNumberTypeID] INT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
