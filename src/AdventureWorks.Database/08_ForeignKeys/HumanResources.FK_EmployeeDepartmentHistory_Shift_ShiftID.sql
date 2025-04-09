
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_EmployeeDepartmentHistory_Shift_ShiftID' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[EmployeeDepartmentHistory] WITH CHECK ADD CONSTRAINT [FK_EmployeeDepartmentHistory_Shift_ShiftID] FOREIGN KEY ([ShiftID])
    REFERENCES [HumanResources].[Shift] ([ShiftID])
GO
