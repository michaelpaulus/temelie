-- BillOfMaterials

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'BillOfMaterials' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[BillOfMaterials]
    (
        [BillOfMaterialsID] INT IDENTITY (1, 1) NOT NULL,
        [ProductAssemblyID] INT NULL,
        [ComponentID] INT NOT NULL,
        [StartDate] DATETIME NOT NULL DEFAULT (GETDATE()),
        [EndDate] DATETIME NULL,
        [UnitMeasureCode] NCHAR(3) NOT NULL,
        [BOMLevel] SMALLINT NOT NULL,
        [PerAssemblyQty] DECIMAL(8, 2) NOT NULL DEFAULT (1.00),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Items required to make bicycles and bicycle subassemblies. It identifies the heirarchical relationship between a parent product and its components.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'BillOfMaterialsID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'BillOfMaterialsID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for BillOfMaterials records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'BillOfMaterialsID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'ProductAssemblyID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'ProductAssemblyID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Parent product identification number. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'ProductAssemblyID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'ComponentID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'ComponentID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Component identification number. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'ComponentID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'StartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'StartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the component started being used in the assembly item.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'StartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'EndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'EndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the component stopped being used in the assembly item.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'EndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'UnitMeasureCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'UnitMeasureCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Standard code identifying the unit of measure for the quantity.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'UnitMeasureCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'BOMLevel')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'BOMLevel';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Indicates the depth the component is from its parent (AssemblyID).',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'BOMLevel';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'PerAssemblyQty')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'PerAssemblyQty';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Quantity of the component needed to create the assembly.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'PerAssemblyQty';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'BillOfMaterials', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'BillOfMaterials',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'BillOfMaterials',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

