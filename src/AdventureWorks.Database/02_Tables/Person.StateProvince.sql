-- StateProvince

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'StateProvince' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[StateProvince]
    (
        [StateProvinceID] INT IDENTITY (1, 1) NOT NULL,
        [StateProvinceCode] NCHAR(3) NOT NULL,
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [IsOnlyStateProvinceFlag] FLAG NOT NULL DEFAULT (1),
        [Name] NAME NOT NULL,
        [TerritoryID] INT NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
