-- EmployeePayHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'EmployeePayHistory' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[EmployeePayHistory]
    (
        [BusinessEntityID] INT NOT NULL,
        [RateChangeDate] DATETIME NOT NULL,
        [Rate] MONEY NOT NULL,
        [PayFrequency] TINYINT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
