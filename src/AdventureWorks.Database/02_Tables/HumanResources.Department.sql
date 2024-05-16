-- Department

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Department' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[Department]
    (
        [DepartmentID] SMALLINT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [GroupName] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
