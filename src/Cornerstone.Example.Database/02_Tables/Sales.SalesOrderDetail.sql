-- SalesOrderDetail

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesOrderDetail' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesOrderDetail]
    (
        [SalesOrderID] INT NOT NULL,
        [SalesOrderDetailID] INT IDENTITY (1, 1) NOT NULL,
        [CarrierTrackingNumber] NVARCHAR(25) NULL,
        [OrderQty] SMALLINT NOT NULL,
        [ProductID] INT NOT NULL,
        [SpecialOfferID] INT NOT NULL,
        [UnitPrice] MONEY NOT NULL,
        [UnitPriceDiscount] MONEY NOT NULL DEFAULT (0.0),
        [LineTotal] AS (isnull(([UnitPrice]*((1.0)-[UnitPriceDiscount]))*[OrderQty],(0.0))),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Individual products associated with a specific sales order. See SalesOrderHeader.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'SalesOrderID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'SalesOrderID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to SalesOrderHeader.SalesOrderID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'SalesOrderID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'SalesOrderDetailID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'SalesOrderDetailID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. One incremental unique number per product sold.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'SalesOrderDetailID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'CarrierTrackingNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'CarrierTrackingNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipment tracking number supplied by the shipper.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'CarrierTrackingNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'OrderQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'OrderQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity ordered per product.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'OrderQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product sold to customer. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'SpecialOfferID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'SpecialOfferID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Promotional code. Foreign key to SpecialOffer.SpecialOfferID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'SpecialOfferID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'UnitPrice')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'UnitPrice';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Selling price of a single product.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'UnitPrice';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'UnitPriceDiscount')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'UnitPriceDiscount';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Discount amount.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'UnitPriceDiscount';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'LineTotal')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'LineTotal';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Per product subtotal. Computed as UnitPrice * (1 - UnitPriceDiscount) * OrderQty.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'LineTotal';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderDetail', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderDetail',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderDetail',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

