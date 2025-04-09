
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_EmployeeDepartmentHistory_Employee_BusinessEntityID' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] WITH CHECK ADD CONSTRAINT [FK_EmployeeDepartmentHistory_Employee_BusinessEntityID] FOREIGN KEY ([BusinessEntityID])
    REFERENCES [HumanResources].[Employee] ([BusinessEntityID])
GO
