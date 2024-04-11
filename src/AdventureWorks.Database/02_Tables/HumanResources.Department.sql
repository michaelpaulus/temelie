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

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Department', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Department';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Lookup table containing the departments within the Adventure Works Cycles company.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Department';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Department', 'column', 'DepartmentID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Department',
                                     @level2type = N'column',
                                     @level2name = 'DepartmentID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Department records.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Department',
                                @level2type = N'column',
                                @level2name = 'DepartmentID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Department', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Department',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Name of the department.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Department',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Department', 'column', 'GroupName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Department',
                                     @level2type = N'column',
                                     @level2name = 'GroupName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Name of the group to which the department belongs.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Department',
                                @level2type = N'column',
                                @level2name = 'GroupName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'HumanResources', 'table', 'Department', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'HumanResources',
                                     @level1type = N'table',
                                     @level1name = 'Department',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'HumanResources',
                                @level1type = N'table',
                                @level1name = 'Department',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

