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
            indexes.name = 'AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate' AND
            schemas.name = 'Production'
    )
    CREATE UNIQUE CLUSTERED INDEX [AK_BillOfMaterials_ProductAssemblyID_ComponentID_StartDate] ON [Production].[BillOfMaterials]
    (
    [ProductAssemblyID],
    [ComponentID],
    [StartDate]
    )
GO
