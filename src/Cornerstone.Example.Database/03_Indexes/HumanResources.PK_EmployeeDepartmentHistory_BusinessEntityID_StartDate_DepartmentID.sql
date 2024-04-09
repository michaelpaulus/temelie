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
            indexes.name = 'PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] ADD CONSTRAINT [PK_EmployeeDepartmentHistory_BusinessEntityID_StartDate_DepartmentID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [StartDate],
    [DepartmentID],
    [ShiftID]
    )
GO
