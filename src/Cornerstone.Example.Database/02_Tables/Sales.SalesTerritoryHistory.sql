-- SalesTerritoryHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesTerritoryHistory' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesTerritoryHistory]
    (
        [BusinessEntityID] INT NOT NULL,
        [TerritoryID] INT NOT NULL,
        [StartDate] DATETIME NOT NULL,
        [EndDate] DATETIME NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales representative transfers to other sales territories.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. The sales rep.  Foreign key to SalesPerson.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'TerritoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'TerritoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Territory identification number. Foreign key to SalesTerritory.SalesTerritoryID.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'TerritoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'StartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'StartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Date the sales representive started work in the territory.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'StartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'EndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'EndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the sales representative left work in the territory.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'EndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesTerritoryHistory', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesTerritoryHistory',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesTerritoryHistory',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

