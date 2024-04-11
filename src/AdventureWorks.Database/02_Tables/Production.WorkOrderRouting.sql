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

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Work order details.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'WorkOrderID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'WorkOrderID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to WorkOrder.WorkOrderID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'WorkOrderID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'OperationSequence')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'OperationSequence';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Indicates the manufacturing process sequence.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'OperationSequence';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'LocationID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'LocationID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Manufacturing location where the part is processed. Foreign key to Location.LocationID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'LocationID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ScheduledStartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ScheduledStartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Planned manufacturing start date.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ScheduledStartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ScheduledEndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ScheduledEndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Planned manufacturing end date.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ScheduledEndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ActualStartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ActualStartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Actual start date.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ActualStartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ActualEndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ActualEndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Actual end date.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ActualEndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ActualResourceHrs')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ActualResourceHrs';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Number of manufacturing hours used.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ActualResourceHrs';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'PlannedCost')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'PlannedCost';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Estimated manufacturing cost.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'PlannedCost';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ActualCost')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ActualCost';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Actual manufacturing cost.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ActualCost';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'WorkOrderRouting', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'WorkOrderRouting',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'WorkOrderRouting',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

