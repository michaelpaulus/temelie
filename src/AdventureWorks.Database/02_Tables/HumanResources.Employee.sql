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

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee information such as salary, department, and title.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Employee records.  Foreign key to BusinessEntity.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'NationalIDNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'NationalIDNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unique national identification number such as a social security number.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'NationalIDNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'LoginID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'LoginID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Network login.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'LoginID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'OrganizationNode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'OrganizationNode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Where the employee is located in corporate hierarchy.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'OrganizationNode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'OrganizationLevel')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'OrganizationLevel';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The depth of the employee in the corporate hierarchy.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'OrganizationLevel';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'JobTitle')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'JobTitle';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Work title such as Buyer or Sales Representative.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'JobTitle';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'BirthDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'BirthDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date of birth.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'BirthDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'MaritalStatus')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'MaritalStatus';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'M = Married, S = Single',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'MaritalStatus';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'Gender')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'Gender';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'M = Male, F = Female',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'Gender';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'HireDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'HireDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee hired on this date.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'HireDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'SalariedFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'SalariedFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Job classification. 0 = Hourly, not exempt from collective bargaining. 1 = Salaried, exempt from collective bargaining.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'SalariedFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'VacationHours')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'VacationHours';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Number of available vacation hours.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'VacationHours';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'SickLeaveHours')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'SickLeaveHours';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Number of available sick leave hours.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'SickLeaveHours';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'CurrentFlag')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'CurrentFlag';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Inactive, 1 = Active',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'CurrentFlag';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Employee', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Employee',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Employee',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

