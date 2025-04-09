
IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.foreign_keys INNER JOIN
            sys.schemas ON
                foreign_keys.schema_id = schemas.schema_id
        WHERE
            foreign_keys.name = 'FK_WorkOrder_ScrapReason_ScrapReasonID' AND
            schemas.name = 'Production'
    )
    ALTER TABLE [Production].[WorkOrder] WITH CHECK ADD CONSTRAINT [FK_WorkOrder_ScrapReason_ScrapReasonID] FOREIGN KEY ([ScrapReasonID])
    REFERENCES [Production].[ScrapReason] ([ScrapReasonID])
GO
