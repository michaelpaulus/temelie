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
            indexes.name = 'PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[WorkOrderRouting] ADD CONSTRAINT [PK_WorkOrderRouting_WorkOrderID_ProductID_OperationSequence] PRIMARY KEY CLUSTERED
    (
    [WorkOrderID],
    [ProductID],
    [OperationSequence]
    )
GO
