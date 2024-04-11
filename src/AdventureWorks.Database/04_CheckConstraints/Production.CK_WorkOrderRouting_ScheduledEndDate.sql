
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
        check_constraints.name = 'CK_WorkOrderRouting_ScheduledEndDate' AND
        schemas.name = 'Production'
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
            check_constraints.name = 'CK_WorkOrderRouting_ScheduledEndDate' AND
            schemas.name = 'Production' AND
            check_constraints.definition = '([ScheduledEndDate]>=[ScheduledStartDate])'
    )
BEGIN
ALTER TABLE [Production].[WorkOrderRouting] DROP CONSTRAINT [CK_WorkOrderRouting_ScheduledEndDate]
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
        check_constraints.name = 'CK_WorkOrderRouting_ScheduledEndDate' AND
        schemas.name = 'Production'
)
ALTER TABLE [Production].[WorkOrderRouting] WITH CHECK ADD CONSTRAINT [CK_WorkOrderRouting_ScheduledEndDate] CHECK (([ScheduledEndDate]>=[ScheduledStartDate]))
GO
