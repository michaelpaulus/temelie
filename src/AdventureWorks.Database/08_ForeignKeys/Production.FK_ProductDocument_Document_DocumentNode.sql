
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_ProductDocument_Document_DocumentNode' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[ProductDocument] WITH CHECK ADD CONSTRAINT [FK_ProductDocument_Document_DocumentNode] FOREIGN KEY ([DocumentNode])
    REFERENCES [Production].[Document] ([DocumentNode])
GO
