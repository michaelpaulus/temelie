
IF NOT EXISTS
(
    SELECT
        1
    FROM
        sys.check_constraints INNER JOIN
        sys.tables ON
            check_constraints.parent_object_id = tables.object_id INNER JOIN
        sys.schemas ON
            tables.schema_id = schemas.schema_id
    WHERE
        check_constraints.name = 'CK_Employee_VacationHours' AND
        schemas.name = 'HumanResources'
)
ALTER TABLE [HumanResources].[Employee] WITH CHECK ADD CONSTRAINT [CK_Employee_VacationHours] CHECK (([VacationHours]>=(-40) AND [VacationHours]<=(240)))
GO
