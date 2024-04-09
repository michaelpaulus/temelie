-- ErrorLog

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ErrorLog' AND
            schemas.name = 'dbo'
    )
    CREATE TABLE [dbo].[ErrorLog]
    (
        [ErrorLogID] INT IDENTITY (1, 1) NOT NULL,
        [ErrorTime] DATETIME NOT NULL DEFAULT (GETDATE()),
        [UserName] SYSNAME NOT NULL,
        [ErrorNumber] INT NOT NULL,
        [ErrorSeverity] INT NULL,
        [ErrorState] INT NULL,
        [ErrorProcedure] NVARCHAR(126) NULL,
        [ErrorLine] INT NULL,
        [ErrorMessage] NVARCHAR(4000) NOT NULL
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Audit table tracking errors in the the AdventureWorks database that are caught by the CATCH block of a TRY...CATCH construct. Data is inserted by stored procedure dbo.uspLogError when it is executed from inside the CATCH block of a TRY...CATCH construct.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorLogID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorLogID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for ErrorLog records.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorLogID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorTime')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorTime';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The date and time at which the error occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorTime';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'UserName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'UserName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The user who executed the batch in which the error occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'UserName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The error number of the error that occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorSeverity')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorSeverity';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The severity of the error that occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorSeverity';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorState')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorState';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The state number of the error that occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorState';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorProcedure')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorProcedure';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The name of the stored procedure or trigger where the error occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorProcedure';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorLine')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorLine';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The line number at which the error occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorLine';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'dbo', 'table', 'ErrorLog', 'column', 'ErrorMessage')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'dbo',
                                     @level1type = N'table',
                                     @level1name = 'ErrorLog',
                                     @level2type = N'column',
                                     @level2name = 'ErrorMessage';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'The message text of the error that occurred.',
                                @level0type = N'schema',
                                @level0name = 'dbo',
                                @level1type = N'table',
                                @level1name = 'ErrorLog',
                                @level2type = N'column',
                                @level2name = 'ErrorMessage';
GO

