IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.indexes INNER JOIN
            sys.tables ON
                indexes.object_id = tables.object_id INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            indexes.name = 'PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesTerritoryHistory] ADD CONSTRAINT [PK_SalesTerritoryHistory_BusinessEntityID_StartDate_TerritoryID] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [StartDate],
    [TerritoryID]
    )
GO
