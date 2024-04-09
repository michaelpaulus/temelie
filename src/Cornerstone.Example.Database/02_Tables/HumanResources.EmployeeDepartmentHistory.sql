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

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee department transfers.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Employee identification number. Foreign key to Employee.BusinessEntityID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'DepartmentID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'DepartmentID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Department in which the employee worked including currently. Foreign key to Department.DepartmentID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'DepartmentID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'ShiftID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'ShiftID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Identifies which 8-hour shift the employee works. Foreign key to Shift.Shift.ID.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'ShiftID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'StartDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'StartDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the employee started work in the department.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'StartDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'EndDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'EndDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date the employee left the department. NULL = Current department.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'EndDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'EmployeeDepartmentHistory', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'EmployeeDepartmentHistory',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'EmployeeDepartmentHistory',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

