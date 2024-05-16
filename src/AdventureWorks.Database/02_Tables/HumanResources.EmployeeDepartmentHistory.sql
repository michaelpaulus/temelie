-- EmployeeDepartmentHistory

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'EmployeeDepartmentHistory' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[EmployeeDepartmentHistory]
    (
        [BusinessEntityID] INT NOT NULL,
        [DepartmentID] SMALLINT NOT NULL,
        [ShiftID] TINYINT NOT NULL,
        [StartDate] DATE NOT NULL,
        [EndDate] DATE NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
