-- CountryRegion

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CountryRegion' AND
            schemas.name = 'Person'
    )
    CREATE TABLE [Person].[CountryRegion]
    (
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'CountryRegion', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegion';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Lookup table containing the ISO standard codes for countries and regions.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'CountryRegion';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'CountryRegion', 'column', 'CountryRegionCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegion',
                                     @level2type = N'column',
                                     @level2name = 'CountryRegionCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ISO standard code for countries and regions.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'CountryRegion',
                                @level2type = N'column',
                                @level2name = 'CountryRegionCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'CountryRegion', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegion',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Country or region name.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'CountryRegion',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Person', 'table', 'CountryRegion', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Person',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegion',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Person',
                                @level1type = N'table',
                                @level1name = 'CountryRegion',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

