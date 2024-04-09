
IF EXISTS
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
        check_constraints.name = 'CK_EmployeePayHistory_Rate' AND
        schemas.name = 'HumanResources'
)
BEGIN
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
            check_constraints.name = 'CK_EmployeePayHistory_Rate' AND
            schemas.name = 'HumanResources' AND
            check_constraints.definition = '([Rate]>=(6.50) AND [Rate]<=(200.00))'
    )
BEGIN
ALTER TABLE [HumanResources].[EmployeePayHistory] DROP CONSTRAINT [CK_EmployeePayHistory_Rate]
END
END
GO

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
        check_constraints.name = 'CK_EmployeePayHistory_Rate' AND
        schemas.name = 'HumanResources'
)
ALTER TABLE [HumanResources].[EmployeePayHistory] WITH CHECK ADD CONSTRAINT [CK_EmployeePayHistory_Rate] CHECK (([Rate]>=(6.50) AND [Rate]<=(200.00)))
GO
