﻿
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[BusinessEntityAddress] WITH CHECK ADD CONSTRAINT [FK_BusinessEntityAddress_BusinessEntity_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [Person].[BusinessEntity] ([BusinessEntityID])
GO
