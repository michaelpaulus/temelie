-- WorkOrderRouting

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'WorkOrderRouting' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[WorkOrderRouting]
    (
        [WorkOrderID] INT NOT NULL,
        [ProductID] INT NOT NULL,
        [OperationSequence] SMALLINT NOT NULL,
        [LocationID] SMALLINT NOT NULL,
        [ScheduledStartDate] DATETIME NOT NULL,
        [ScheduledEndDate] DATETIME NOT NULL,
        [ActualStartDate] DATETIME NULL,
        [ActualEndDate] DATETIME NULL,
        [ActualResourceHrs] DECIMAL(9, 4) NULL,
        [PlannedCost] MONEY NOT NULL,
        [ActualCost] MONEY NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
