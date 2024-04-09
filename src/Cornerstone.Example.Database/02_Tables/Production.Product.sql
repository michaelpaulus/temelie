-- Product

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Product' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Product]
    (
        [ProductID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ProductNumber] NVARCHAR(25) NOT NULL,
        [MakeFlag] FLAG NOT NULL DEFAULT (1),
        [FinishedGoodsFlag] FLAG NOT NULL DEFAULT (1),
        [Color] NVARCHAR(15) NULL,
        [SafetyStockLevel] SMALLINT NOT NULL,
        [ReorderPoint] SMALLINT NOT NULL,
        [StandardCost] MONEY NOT NULL,
        [ListPrice] MONEY NOT NULL,
        [Size] NVARCHAR(5) NULL,
        [SizeUnitMeasureCode] NCHAR(3) NULL,
        [WeightUnitMeasureCode] NCHAR(3) NULL,
        [Weight] DECIMAL(8, 2) NULL,
        [DaysToManufacture] INT NOT NULL,
        [ProductLine] NCHAR(2) NULL,
        [Class] NCHAR(2) NULL,
        [Style] NCHAR(2) NULL,
        [ProductSubcategoryID] INT NULL,
        [ProductModelID] INT NULL,
        [SellStartDate] DATETIME NOT NULL,
        [SellEndDate] DATETIME NULL,
        [DiscontinuedDate] DATETIME NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Products sold or used in the manfacturing of sold products.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Product records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Name of the product.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ProductNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ProductNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unique product identification number.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ProductNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'MakeFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'MakeFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Product is purchased, 1 = Product is manufactured in-house.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'MakeFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'FinishedGoodsFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'FinishedGoodsFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Product is not a salable item. 1 = Product is salable.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'FinishedGoodsFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Color')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Color';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product color.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Color';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'SafetyStockLevel')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'SafetyStockLevel';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Minimum inventory quantity. ',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'SafetyStockLevel';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ReorderPoint')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ReorderPoint';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Inventory level that triggers a purchase order or work order. ',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ReorderPoint';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'StandardCost')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'StandardCost';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Standard cost of the product.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'StandardCost';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ListPrice')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ListPrice';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Selling price.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ListPrice';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Size')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Size';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product size.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Size';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'SizeUnitMeasureCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'SizeUnitMeasureCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unit of measure for Size column.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'SizeUnitMeasureCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'WeightUnitMeasureCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'WeightUnitMeasureCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unit of measure for Weight column.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'WeightUnitMeasureCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Weight')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Weight';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product weight.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Weight';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'DaysToManufacture')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'DaysToManufacture';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Number of days required to manufacture the product.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'DaysToManufacture';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ProductLine')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ProductLine';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'R = Road, M = Mountain, T = Touring, S = Standard',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ProductLine';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Class')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Class';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'H = High, M = Medium, L = Low',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Class';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'Style')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'Style';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'W = Womens, M = Mens, U = Universal',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'Style';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ProductSubcategoryID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ProductSubcategoryID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product is a member of this product subcategory. Foreign key to ProductSubCategory.ProductSubCategoryID. ',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ProductSubcategoryID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ProductModelID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ProductModelID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product is a member of this product model. Foreign key to ProductModel.ProductModelID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ProductModelID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'SellStartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'SellStartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the product was available for sale.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'SellStartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'SellEndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'SellEndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the product was no longer available for sale.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'SellEndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'DiscontinuedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'DiscontinuedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the product was discontinued.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'DiscontinuedDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Product', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Product',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Product',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

