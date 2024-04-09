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
            indexes.name = 'PK_EmployeePayHistory_BusinessEntityID_RateChangeDate' AND
            schemas.name = 'HumanResources'
    )
    ALTER TABLE [HumanResources].[EmployeePayHistory] ADD CONSTRAINT [PK_EmployeePayHistory_BusinessEntityID_RateChangeDate] PRIMARY KEY CLUSTERED
    (
    [BusinessEntityID],
    [RateChangeDate]
    )
GO
