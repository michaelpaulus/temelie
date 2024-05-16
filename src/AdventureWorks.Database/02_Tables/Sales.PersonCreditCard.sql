-- PersonCreditCard

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PersonCreditCard' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[PersonCreditCard]
    (
        [BusinessEntityID] INT NOT NULL,
        [CreditCardID] INT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
