-- SalesPerson

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesPerson' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesPerson]
    (
        [BusinessEntityID] INT NOT NULL,
        [TerritoryID] INT NULL,
        [SalesQuota] MONEY NULL,
        [Bonus] MONEY NOT NULL DEFAULT (0.00),
        [CommissionPct] SMALLMONEY NOT NULL DEFAULT (0.00),
        [SalesYTD] MONEY NOT NULL DEFAULT (0.00),
        [SalesLastYear] MONEY NOT NULL DEFAULT (0.00),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales representative current information.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for SalesPerson records. Foreign key to Employee.BusinessEntityID',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'TerritoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'TerritoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Territory currently assigned to. Foreign key to SalesTerritory.SalesTerritoryID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'TerritoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'SalesQuota')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'SalesQuota';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Projected yearly sales.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'SalesQuota';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'Bonus')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'Bonus';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Bonus due if quota is met.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'Bonus';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'CommissionPct')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'CommissionPct';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Commision percent received per sale.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'CommissionPct';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'SalesYTD')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'SalesYTD';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales total year to date.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'SalesYTD';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'SalesLastYear')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'SalesLastYear';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales total of previous year.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'SalesLastYear';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesPerson', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesPerson',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesPerson',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

