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
            indexes.name = 'PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate' AND
            schemas.name = 'Sales'
    )
    ALTER TABLE [Sales].[SalesPersonQuotaHistory] ADD CONSTRAINT [PK_SalesPersonQuotaHistory_BusinessEntityID_QuotaDate] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [QuotaDate]
    )
GO
