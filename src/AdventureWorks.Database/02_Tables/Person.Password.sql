-- Password

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Password' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[Password]
    (
        [BusinessEntityID] INT NOT NULL,
        [PasswordHash] VARCHAR(128) NOT NULL,
        [PasswordSalt] VARCHAR(10) NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
