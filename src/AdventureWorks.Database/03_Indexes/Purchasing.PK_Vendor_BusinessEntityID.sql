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
            indexes.name = 'PK_Vendor_BusinessEntityID' AND
            schemas.name = 'Purchasing'
    )
    ALTER TABLE [Purchasing].[Vendor] ADD CONSTRAINT [PK_Vendor_BusinessEntityID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID]
    )
GO
