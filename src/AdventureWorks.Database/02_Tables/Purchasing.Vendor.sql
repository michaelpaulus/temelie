-- Vendor

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Vendor' AND
            schemas.name = 'Purchasing'
    )
    CREATE TABLE [Purchasing].[Vendor]
    (
        [BusinessEntityID] INT NOT NULL,
        [AccountNumber] ACCOUNTNUMBER NOT NULL,
        [Name] NAME NOT NULL,
        [CreditRating] TINYINT NOT NULL,
        [PreferredVendorStatus] FLAG NOT NULL DEFAULT (1),
        [ActiveFlag] FLAG NOT NULL DEFAULT (1),
        [PurchasingWebServiceURL] NVARCHAR(1024) NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Companies from whom Adventure Works Cycles purchases parts or other goods.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Vendor records.  Foreign key to BusinessEntity.BusinessEntityID',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'AccountNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'AccountNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Vendor account (identification) number.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'AccountNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Company name.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'CreditRating')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'CreditRating';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'1 = Superior, 2 = Excellent, 3 = Above average, 4 = Average, 5 = Below average',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'CreditRating';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'PreferredVendorStatus')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'PreferredVendorStatus';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Do not use if another vendor is available. 1 = Preferred over other vendors supplying the same product.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'PreferredVendorStatus';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'ActiveFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'ActiveFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Vendor no longer used. 1 = Vendor is actively used.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'ActiveFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'PurchasingWebServiceURL')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'PurchasingWebServiceURL';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Vendor URL.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'PurchasingWebServiceURL';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Purchasing', 'table', 'Vendor', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Purchasing',
                                     @level1type = N'table',
                                     @level1name = 'Vendor',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Purchasing',
                                @level1type = N'table',
                                @level1name = 'Vendor',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

