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
