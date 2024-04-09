-- DatabaseLog

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'DatabaseLog' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[DatabaseLog]
    (
        [DatabaseLogID] INT IDENTITY (1, 1) NOT NULL,
        [PostTime] DATETIME NOT NULL,
        [DatabaseUser] SYSNAME NOT NULL,
        [Event] SYSNAME NOT NULL,
        [Schema] SYSNAME NULL,
        [Object] SYSNAME NULL,
        [TSQL] NVARCHAR(MAX) NOT NULL,
        [XmlEvent] XML NOT NULL
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Audit table tracking all DDL changes made to the AdventureWorks database. Data is captured by the database trigger ddlDatabaseTriggerLog.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'DatabaseLogID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'DatabaseLogID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for DatabaseLog records.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'DatabaseLogID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'PostTime')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'PostTime';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The date and time the DDL change occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'PostTime';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'DatabaseUser')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'DatabaseUser';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The user who implemented the DDL change.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'DatabaseUser';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'Event')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'Event';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The type of DDL statement that was executed.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'Event';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'Schema')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'Schema';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The schema to which the changed object belongs.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'Schema';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'Object')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'Object';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The object that was changed by the DDL statment.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'Object';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'TSQL')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'TSQL';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The exact Transact-SQL statement that was executed.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'TSQL';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'DatabaseLog', 'column', 'XmlEvent')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'DatabaseLog',
                                     @level2type = N'column',
                                     @level2name = 'XmlEvent';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The raw XML data generated by database trigger.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'DatabaseLog',
                                @level2type = N'column',
                                @level2name = 'XmlEvent';
GO

