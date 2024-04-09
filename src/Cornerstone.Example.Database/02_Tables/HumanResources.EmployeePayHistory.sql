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

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee pay history.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee identification number. Foreign key to Employee.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', 'column', 'RateChangeDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory',
                                     @level2type = N'column',
                                     @level2name = 'RateChangeDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the change in pay is effective',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory',
                                @level2type = N'column',
                                @level2name = 'RateChangeDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', 'column', 'Rate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory',
                                     @level2type = N'column',
                                     @level2name = 'Rate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Salary hourly rate.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory',
                                @level2type = N'column',
                                @level2name = 'Rate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', 'column', 'PayFrequency')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory',
                                     @level2type = N'column',
                                     @level2name = 'PayFrequency';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'1 = Salary received monthly, 2 = Salary received biweekly',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory',
                                @level2type = N'column',
                                @level2name = 'PayFrequency';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeePayHistory', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeePayHistory',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeePayHistory',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

