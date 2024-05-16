-- PhoneNumberType

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PhoneNumberType' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[PhoneNumberType]
    (
        [PhoneNumberTypeID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
