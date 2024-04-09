-- PhoneNumberType

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'PhoneNumberType' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[PhoneNumberType]
    (
        [PhoneNumberTypeID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'PhoneNumberType', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'PhoneNumberType';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Type of phone number of a person.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'PhoneNumberType';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'PhoneNumberType', 'column', 'PhoneNumberTypeID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'PhoneNumberType',
                                     @level2type = N'column',
                                     @level2name = 'PhoneNumberTypeID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for telephone number type records.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'PhoneNumberType',
                                @level2type = N'column',
                                @level2name = 'PhoneNumberTypeID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'PhoneNumberType', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'PhoneNumberType',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Name of the telephone number type',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'PhoneNumberType',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'PhoneNumberType', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'PhoneNumberType',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'PhoneNumberType',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

