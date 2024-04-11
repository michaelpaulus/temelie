-- SalesOrderHeader

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesOrderHeader' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesOrderHeader]
    (
        [SalesOrderID] INT IDENTITY (1, 1) NOT NULL,
        [RevisionNumber] TINYINT NOT NULL DEFAULT (0),
        [OrderDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [DueDate] DATETIME NOT NULL,
        [ShipDate] DATETIME NULL,
        [Status] TINYINT NOT NULL DEFAULT (1),
        [OnlineOrderFlag] FLAG NOT NULL DEFAULT (1),
        [SalesOrderNumber] AS (isnull(N'SO'+CONVERT([nvarchar](23),[SalesOrderID]),N'*** ERROR ***')),
        [PurchaseOrderNumber] ORDERNUMBER NULL,
        [AccountNumber] ACCOUNTNUMBER NULL,
        [CustomerID] INT NOT NULL,
        [SalesPersonID] INT NULL,
        [TerritoryID] INT NULL,
        [BillToAddressID] INT NOT NULL,
        [ShipToAddressID] INT NOT NULL,
        [ShipMethodID] INT NOT NULL,
        [CreditCardID] INT NULL,
        [CreditCardApprovalCode] VARCHAR(15) NULL,
        [CurrencyRateID] INT NULL,
        [SubTotal] MONEY NOT NULL DEFAULT (0.00),
        [TaxAmt] MONEY NOT NULL DEFAULT (0.00),
        [Freight] MONEY NOT NULL DEFAULT (0.00),
        [TotalDue] AS (isnull(([SubTotal]+[TaxAmt])+[Freight],(0))),
        [Comment] NVARCHAR(128) NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'General sales order information.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'SalesOrderID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'SalesOrderID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'SalesOrderID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'RevisionNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'RevisionNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Incremental number to track changes to the sales order over time.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'RevisionNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'OrderDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'OrderDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Dates the sales order was created.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'OrderDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'DueDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'DueDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the order is due to the customer.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'DueDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'ShipDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ShipDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the order was shipped to the customer.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ShipDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'Status')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'Status';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Order current status. 1 = In process; 2 = Approved; 3 = Backordered; 4 = Rejected; 5 = Shipped; 6 = Cancelled',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'Status';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'OnlineOrderFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'OnlineOrderFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Order placed by sales person. 1 = Order placed online by customer.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'OnlineOrderFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'SalesOrderNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'SalesOrderNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unique sales order identification number.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'SalesOrderNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'PurchaseOrderNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'PurchaseOrderNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Customer purchase order number reference. ',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'PurchaseOrderNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'AccountNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'AccountNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Financial accounting number reference.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'AccountNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'CustomerID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'CustomerID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Customer identification number. Foreign key to Customer.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'CustomerID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'SalesPersonID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'SalesPersonID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales person who created the sales order. Foreign key to SalesPerson.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'SalesPersonID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'TerritoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'TerritoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Territory in which the sale was made. Foreign key to SalesTerritory.SalesTerritoryID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'TerritoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'BillToAddressID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'BillToAddressID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Customer billing address. Foreign key to Address.AddressID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'BillToAddressID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'ShipToAddressID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ShipToAddressID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Customer shipping address. Foreign key to Address.AddressID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ShipToAddressID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'ShipMethodID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ShipMethodID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping method. Foreign key to ShipMethod.ShipMethodID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ShipMethodID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'CreditCardID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'CreditCardID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Credit card identification number. Foreign key to CreditCard.CreditCardID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'CreditCardID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'CreditCardApprovalCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'CreditCardApprovalCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Approval code provided by the credit card company.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'CreditCardApprovalCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'CurrencyRateID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'CurrencyRateID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Currency exchange rate used. Foreign key to CurrencyRate.CurrencyRateID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'CurrencyRateID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'SubTotal')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'SubTotal';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales subtotal. Computed as SUM(SalesOrderDetail.LineTotal)for the appropriate SalesOrderID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'SubTotal';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'TaxAmt')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'TaxAmt';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Tax amount.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'TaxAmt';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'Freight')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'Freight';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Shipping cost.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'Freight';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'TotalDue')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'TotalDue';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Total due from customer. Computed as Subtotal + TaxAmt + Freight.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'TotalDue';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'Comment')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'Comment';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales representative comments.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'Comment';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesOrderHeader', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesOrderHeader',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesOrderHeader',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

