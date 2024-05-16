-- CountryRegion

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CountryRegion' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[CountryRegion]
    (
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
