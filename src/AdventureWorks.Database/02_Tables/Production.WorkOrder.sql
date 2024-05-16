-- WorkOrder

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'WorkOrder' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[WorkOrder]
    (
        [WorkOrderID] INT IDENTITY (1, 1) NOT NULL,
        [ProductID] INT NOT NULL,
        [OrderQty] INT NOT NULL,
        [StockedQty] AS (isnull([OrderQty]-[ScrappedQty],(0))),
        [ScrappedQty] SMALLINT NOT NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NULL,
        [DueDate] DATETIME NOT NULL,
        [ScrapReasonID] SMALLINT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
