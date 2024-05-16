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
