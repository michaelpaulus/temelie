-- EmailAddress

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'EmailAddress' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[EmailAddress]
    (
        [BusinessEntityID] INT NOT NULL,
        [EmailAddressID] INT IDENTITY (1, 1) NOT NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
