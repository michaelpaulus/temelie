-- Password

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Password' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[Password]
    (
        [BusinessEntityID] INT NOT NULL,
        [PasswordHash] VARCHAR(128) NOT NULL,
        [PasswordSalt] VARCHAR(10) NOT NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Password', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Password';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'One way hashed authentication information',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Password';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Password', 'column', 'PasswordHash')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Password',
                                     @level2type = N'column',
                                     @level2name = 'PasswordHash';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Password for the e-mail account.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Password',
                                @level2type = N'column',
                                @level2name = 'PasswordHash';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Password', 'column', 'PasswordSalt')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Password',
                                     @level2type = N'column',
                                     @level2name = 'PasswordSalt';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Random value concatenated with the password string before the password is hashed.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Password',
                                @level2type = N'column',
                                @level2name = 'PasswordSalt';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Password', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Password',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Password',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'Password', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'Password',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'Password',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

