IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'PK_PersonCreditCard_BusinessEntityID_CreditCardID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[PersonCreditCard] ADD CONSTRAINT [PK_PersonCreditCard_BusinessEntityID_CreditCardID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [CreditCardID]
    )
GO
