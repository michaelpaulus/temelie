-- Employee

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Employee' AND
            schemas.name = 'HumanResources'
    )
    CREATE TABLE [HumanResources].[Employee]
    (
        [BusinessEntityID] INT NOT NULL,
        [NationalIDNumber] NVARCHAR(15) NOT NULL,
        [LoginID] NVARCHAR(256) NOT NULL,
        [OrganizationNode] HIERARCHYID NULL,
        [OrganizationLevel] AS ([OrganizationNode].[GetLevel]()),
        [JobTitle] NVARCHAR(50) NOT NULL,
        [BirthDate] DATE NOT NULL,
        [MaritalStatus] NCHAR(1) NOT NULL,
        [Gender] NCHAR(1) NOT NULL,
        [HireDate] DATE NOT NULL,
        [SalariedFlag] FLAG NOT NULL DEFAULT (1),
        [VacationHours] SMALLINT NOT NULL DEFAULT (0),
        [SickLeaveHours] SMALLINT NOT NULL DEFAULT (0),
        [CurrentFlag] FLAG NOT NULL DEFAULT (1),
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
