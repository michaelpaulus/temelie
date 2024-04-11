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
            indexes.name = 'PK_Department_DepartmentID' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[Department] ADD CONSTRAINT [PK_Department_DepartmentID] PRIMARY KEY CLUSTERED
    (
    [DepartmentID]
    )
GO
