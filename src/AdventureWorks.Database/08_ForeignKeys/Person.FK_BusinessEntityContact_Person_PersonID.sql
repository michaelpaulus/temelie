
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_BusinessEntityContact_Person_PersonID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[BusinessEntityContact] WITH CHECK ADD CONSTRAINT [FK_BusinessEntityContact_Person_PersonID] FOREIGN KEY ([PersonID])
    REFERENCES [Person].[Person] ([BusinessEntityID])
GO
