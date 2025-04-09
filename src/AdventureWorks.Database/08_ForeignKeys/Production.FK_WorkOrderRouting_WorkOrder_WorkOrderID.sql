
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_WorkOrderRouting_WorkOrder_WorkOrderID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[WorkOrderRouting] WITH CHECK ADD CONSTRAINT [FK_WorkOrderRouting_WorkOrder_WorkOrderID] FOREIGN KEY ([WorkOrderID])
    REFERENCES [Production].[WorkOrder] ([WorkOrderID])
GO
