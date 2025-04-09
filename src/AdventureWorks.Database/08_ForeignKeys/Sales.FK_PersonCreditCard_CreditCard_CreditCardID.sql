
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_PersonCreditCard_CreditCard_CreditCardID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[PersonCreditCard] WITH CHECK ADD CONSTRAINT [FK_PersonCreditCard_CreditCard_CreditCardID] FOREIGN KEY ([CreditCardID])
    REFERENCES [Sales].[CreditCard] ([CreditCardID])
GO
