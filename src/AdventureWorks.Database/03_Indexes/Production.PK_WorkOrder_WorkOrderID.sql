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
            indexes.name = 'PK_WorkOrder_WorkOrderID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[WorkOrder] ADD CONSTRAINT [PK_WorkOrder_WorkOrderID] PRIMARY KEY CLUSTERED
    (
    [WorkOrderID]
    )
GO
