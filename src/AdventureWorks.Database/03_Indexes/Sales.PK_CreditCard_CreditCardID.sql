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
            indexes.name = 'PK_CreditCard_CreditCardID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[CreditCard] ADD CONSTRAINT [PK_CreditCard_CreditCardID] PRIMARY KEY CLUSTERED
    (
    [CreditCardID]
    )
GO
