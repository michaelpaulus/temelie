-- ProductSubcategory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductSubcategory' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductSubcategory]
    (
        [ProductSubcategoryID] INT IDENTITY (1, 1) NOT NULL,
        [ProductCategoryID] INT NOT NULL,
        [Name] NAME NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product subcategories. See ProductCategory table.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', 'column', 'ProductSubcategoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory',
                                     @level2type = N'column',
                                     @level2name = 'ProductSubcategoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for ProductSubcategory records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory',
                                @level2type = N'column',
                                @level2name = 'ProductSubcategoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', 'column', 'ProductCategoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory',
                                     @level2type = N'column',
                                     @level2name = 'ProductCategoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product category identification number. Foreign key to ProductCategory.ProductCategoryID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory',
                                @level2type = N'column',
                                @level2name = 'ProductCategoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Subcategory description.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductSubcategory', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductSubcategory',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductSubcategory',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

