-- CountryRegionCurrency

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CountryRegionCurrency' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CountryRegionCurrency]
    (
        [CountryRegionCode] NVARCHAR(3) NOT NULL,
        [CurrencyCode] NCHAR(3) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CountryRegionCurrency', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegionCurrency';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Cross-reference table mapping ISO currency codes to a country or region.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CountryRegionCurrency';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CountryRegionCurrency', 'column', 'CountryRegionCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegionCurrency',
                                     @level2type = N'column',
                                     @level2name = 'CountryRegionCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ISO code for countries and regions. Foreign key to CountryRegion.CountryRegionCode.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CountryRegionCurrency',
                                @level2type = N'column',
                                @level2name = 'CountryRegionCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CountryRegionCurrency', 'column', 'CurrencyCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegionCurrency',
                                     @level2type = N'column',
                                     @level2name = 'CurrencyCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'ISO standard currency code. Foreign key to Currency.CurrencyCode.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CountryRegionCurrency',
                                @level2type = N'column',
                                @level2name = 'CurrencyCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CountryRegionCurrency', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CountryRegionCurrency',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CountryRegionCurrency',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

