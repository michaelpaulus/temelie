-- PurchaseOrderDetail

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PurchaseOrderDetail' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[PurchaseOrderDetail]
    (
        [PurchaseOrderID] INT NOT NULL,
        [PurchaseOrderDetailID] INT IDENTITY (1, 1) NOT NULL,
        [DueDate] DATETIME NOT NULL,
        [OrderQty] SMALLINT NOT NULL,
        [ProductID] INT NOT NULL,
        [UnitPrice] MONEY NOT NULL,
        [LineTotal] AS (isnull([OrderQty]*[UnitPrice],(0.00))),
        [ReceivedQty] DECIMAL(8, 2) NOT NULL,
        [RejectedQty] DECIMAL(8, 2) NOT NULL,
        [StockedQty] AS (isnull([ReceivedQty]-[RejectedQty],(0.00))),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Individual products associated with a specific purchase order. See PurchaseOrderHeader.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'PurchaseOrderID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'PurchaseOrderID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to PurchaseOrderHeader.PurchaseOrderID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'PurchaseOrderID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'PurchaseOrderDetailID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'PurchaseOrderDetailID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. One line number per purchased product.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'PurchaseOrderDetailID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'DueDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'DueDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the product is expected to be received.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'DueDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'OrderQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'OrderQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity ordered.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'OrderQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product identification number. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'UnitPrice')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'UnitPrice';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Vendor's selling price of a single product.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'UnitPrice';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'LineTotal')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'LineTotal';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Per product subtotal. Computed as OrderQty * UnitPrice.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'LineTotal';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'ReceivedQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'ReceivedQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity actually received from the vendor.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'ReceivedQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'RejectedQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'RejectedQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity rejected during inspection.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'RejectedQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'StockedQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'StockedQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity accepted into inventory. Computed as ReceivedQty - RejectedQty.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'StockedQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderDetail', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderDetail',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

