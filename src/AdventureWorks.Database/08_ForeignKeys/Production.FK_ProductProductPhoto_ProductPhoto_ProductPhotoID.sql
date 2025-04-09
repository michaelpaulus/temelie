
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductProductPhoto_ProductPhoto_ProductPhotoID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductProductPhoto] WITH CHECK ADD CONSTRAINT [FK_ProductProductPhoto_ProductPhoto_ProductPhotoID] FOREIGN KEY ([ProductPhotoID])
    REFERENCES [Production].[ProductPhoto] ([ProductPhotoID])
GO
