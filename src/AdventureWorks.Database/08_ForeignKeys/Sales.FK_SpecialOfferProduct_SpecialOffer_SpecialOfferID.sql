
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SpecialOfferProduct] WITH CHECK ADD CONSTRAINT [FK_SpecialOfferProduct_SpecialOffer_SpecialOfferID] FOREIGN KEY ([SpecialOfferID])
    REFERENCES [Sales].[SpecialOffer] ([SpecialOfferID])
GO
