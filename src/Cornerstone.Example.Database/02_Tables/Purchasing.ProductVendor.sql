-- ProductVendor

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductVendor' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[ProductVendor]
    (
        [ProductID] INT NOT NULL,
        [BusinessEntityID] INT NOT NULL,
        [AverageLeadTime] INT NOT NULL,
        [StandardPrice] MONEY NOT NULL,
        [LastReceiptCost] MONEY NULL,
        [LastReceiptDate] DATETIME NULL,
        [MinOrderQty] INT NOT NULL,
        [MaxOrderQty] INT NOT NULL,
        [OnOrderQty] INT NULL,
        [UnitMeasureCode] NCHAR(3) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Cross-reference table mapping vendors with the products they supply.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to Vendor.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'AverageLeadTime')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'AverageLeadTime';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The average span of time (in days) between placing an order with the vendor and receiving the purchased product.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'AverageLeadTime';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'StandardPrice')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'StandardPrice';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The vendor's usual selling price.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'StandardPrice';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'LastReceiptCost')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'LastReceiptCost';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The selling price when last purchased.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'LastReceiptCost';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'LastReceiptDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'LastReceiptDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the product was last received by the vendor.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'LastReceiptDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'MinOrderQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'MinOrderQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The maximum quantity that should be ordered.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'MinOrderQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'MaxOrderQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'MaxOrderQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The minimum quantity that should be ordered.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'MaxOrderQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'OnOrderQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'OnOrderQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The quantity currently on order.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'OnOrderQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'UnitMeasureCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'UnitMeasureCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The product's unit of measure.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'UnitMeasureCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'ProductVendor', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'ProductVendor',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'ProductVendor',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

