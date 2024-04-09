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
            indexes.name = 'PK_SpecialOfferProduct_SpecialOfferID_ProductID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SpecialOfferProduct] ADD CONSTRAINT [PK_SpecialOfferProduct_SpecialOfferID_ProductID] PRIMARY KEY CLUSTERED
    (
    [SpecialOfferID],
    [ProductID]
    )
GO
