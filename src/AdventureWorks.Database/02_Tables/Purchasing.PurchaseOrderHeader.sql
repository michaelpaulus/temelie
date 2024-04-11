-- PurchaseOrderHeader

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PurchaseOrderHeader' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[PurchaseOrderHeader]
    (
        [PurchaseOrderID] INT IDENTITY (1, 1) NOT NULL,
        [RevisionNumber] TINYINT NOT NULL DEFAULT (0),
        [Status] TINYINT NOT NULL DEFAULT (1),
        [EmployeeID] INT NOT NULL,
        [VendorID] INT NOT NULL,
        [ShipMethodID] INT NOT NULL,
        [OrderDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [ShipDate] DATETIME NULL,
        [SubTotal] MONEY NOT NULL DEFAULT (0.00),
        [TaxAmt] MONEY NOT NULL DEFAULT (0.00),
        [Freight] MONEY NOT NULL DEFAULT (0.00),
        [TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'General purchase order information. See PurchaseOrderDetail.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'PurchaseOrderID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'PurchaseOrderID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'PurchaseOrderID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'RevisionNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'RevisionNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Incremental number to track changes to the purchase order over time.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'RevisionNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'Status')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'Status';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Order current status. 1 = Pending; 2 = Approved; 3 = Rejected; 4 = Complete',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'Status';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'EmployeeID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'EmployeeID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee who created the purchase order. Foreign key to Employee.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'EmployeeID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'VendorID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'VendorID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Vendor with whom the purchase order is placed. Foreign key to Vendor.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'VendorID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'ShipMethodID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ShipMethodID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping method. Foreign key to ShipMethod.ShipMethodID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ShipMethodID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'OrderDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'OrderDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Purchase order creation date.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'OrderDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'ShipDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ShipDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Estimated shipment date from the vendor.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ShipDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'SubTotal')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'SubTotal';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Purchase order subtotal. Computed as SUM(PurchaseOrderDetail.LineTotal)for the appropriate PurchaseOrderID.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'SubTotal';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'TaxAmt')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'TaxAmt';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Tax amount.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'TaxAmt';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'Freight')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'Freight';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping cost.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'Freight';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'TotalDue')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'TotalDue';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Total due to vendor. Computed as Subtotal + TaxAmt + Freight.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'TotalDue';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'PurchaseOrderHeader', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'PurchaseOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'PurchaseOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

