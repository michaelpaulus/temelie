-- CreditCard

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CreditCard' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CreditCard]
    (
        [CreditCardID] INT IDENTITY (1, 1) NOT NULL,
        [CardType] NVARCHAR(50) NOT NULL,
        [CardNumber] NVARCHAR(25) NOT NULL,
        [ExpMonth] TINYINT NOT NULL,
        [ExpYear] SMALLINT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
