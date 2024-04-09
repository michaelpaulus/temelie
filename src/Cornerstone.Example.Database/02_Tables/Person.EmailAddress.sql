-- EmailAddress

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'EmailAddress' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[EmailAddress]
    (
        [BusinessEntityID] INT NOT NULL,
        [EmailAddressID] INT IDENTITY (1, 1) NOT NULL,
        [EmailAddress] NVARCHAR(50) NULL,
        [rowguid] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWID()),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Where to send a person email.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', 'column', 'BusinessEntityID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress',
                                     @level2type = N'column',
                                     @level2name = 'BusinessEntityID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Person associated with this email address.  Foreign key to Person.BusinessEntityID',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress',
                                @level2type = N'column',
                                @level2name = 'BusinessEntityID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', 'column', 'EmailAddressID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress',
                                     @level2type = N'column',
                                     @level2name = 'EmailAddressID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. ID of this email address.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress',
                                @level2type = N'column',
                                @level2name = 'EmailAddressID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', 'column', 'EmailAddress')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress',
                                     @level2type = N'column',
                                     @level2name = 'EmailAddress';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'E-mail address for the person.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress',
                                @level2type = N'column',
                                @level2name = 'EmailAddress';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', 'column', 'rowguid')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress',
                                     @level2type = N'column',
                                     @level2name = 'rowguid';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ROWGUIDCOL number uniquely identifying the record. Used to support a merge replication sample.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress',
                                @level2type = N'column',
                                @level2name = 'rowguid';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'EmailAddress', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'EmailAddress',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'EmailAddress',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

