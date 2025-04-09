
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_Document_Employee_Owner' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[Document] WITH CHECK ADD CONSTRAINT [FK_Document_Employee_Owner] FOREIGN KEY ([Owner])
    REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
