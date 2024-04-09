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
            indexes.name = 'PK_StateProvince_StateProvinceID' AND
            schemas.name = 'Person'
    )
    ALTER TABLE [Person].[StateProvince] ADD CONSTRAINT [PK_StateProvince_StateProvinceID] PRIMARY KEY CLUSTERED
    (
    [StateProvinceID]
    )
GO
