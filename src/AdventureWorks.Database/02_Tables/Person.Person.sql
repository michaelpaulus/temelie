-- Person

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Person' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[Person]
    (
        [BusinessEntityID] INT NOT NULL,
        [PersonType] NCHAR(2) NOT NULL,
        [NameStyle] NAMESTYLE NOT NULL DEFAULT (0),
        [Title] NVARCHAR(8) NULL,
        [FirstName] NAME NOT NULL,
        [MiddleName] NAME NULL,
        [LastName] NAME NOT NULL,
        [Suffix] NVARCHAR(10) NULL,
        [EmailPromotion] INT NOT NULL DEFAULT (0),
        [AdditionalContactInfo] XML NULL,
        [Demographics] XML NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
