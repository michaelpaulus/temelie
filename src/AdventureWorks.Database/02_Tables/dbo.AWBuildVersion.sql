-- AWBuildVersion

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'AWBuildVersion' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[AWBuildVersion]
    (
        [SystemInformationID] TINYINT IDENTITY (1, 1) NOT NULL,
        [Database Version] NVARCHAR(25) NOT NULL,
        [VersionDate] DATETIME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'AWBuildVersion', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'AWBuildVersion';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Current version number of the AdventureWorks 2016 sample database. ',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'AWBuildVersion';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'AWBuildVersion', 'column', 'SystemInformationID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'AWBuildVersion',
                                     @level2type = N'column',
                                     @level2name = 'SystemInformationID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for AWBuildVersion records.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'AWBuildVersion',
                                @level2type = N'column',
                                @level2name = 'SystemInformationID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'AWBuildVersion', 'column', 'Database Version')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'AWBuildVersion',
                                     @level2type = N'column',
                                     @level2name = 'Database Version';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Version number of the database in 9.yy.mm.dd.00 format.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'AWBuildVersion',
                                @level2type = N'column',
                                @level2name = 'Database Version';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'AWBuildVersion', 'column', 'VersionDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'AWBuildVersion',
                                     @level2type = N'column',
                                     @level2name = 'VersionDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'AWBuildVersion',
                                @level2type = N'column',
                                @level2name = 'VersionDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'AWBuildVersion', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'AWBuildVersion',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'AWBuildVersion',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

