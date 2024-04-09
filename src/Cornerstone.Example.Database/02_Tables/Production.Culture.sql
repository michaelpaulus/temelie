-- Culture

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'Culture' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[Culture]
    (
        [CultureID] NCHAR(6) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Culture', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Culture';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Lookup table containing the languages in which some AdventureWorks data is stored.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Culture';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Culture', 'column', 'CultureID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Culture',
                                     @level2type = N'column',
                                     @level2name = 'CultureID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for Culture records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Culture',
                                @level2type = N'column',
                                @level2name = 'CultureID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Culture', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Culture',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Culture description.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Culture',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'Culture', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'Culture',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'Culture',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

