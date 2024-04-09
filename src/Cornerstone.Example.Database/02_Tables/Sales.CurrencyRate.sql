-- CurrencyRate

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CurrencyRate' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CurrencyRate]
    (
        [CurrencyRateID] INT IDENTITY (1, 1) NOT NULL,
        [CurrencyRateDate] DATETIME NOT NULL,
        [FromCurrencyCode] NCHAR(3) NOT NULL,
        [ToCurrencyCode] NCHAR(3) NOT NULL,
        [AverageRate] MONEY NOT NULL,
        [EndOfDayRate] MONEY NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Currency exchange rates.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'CurrencyRateID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'CurrencyRateID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for CurrencyRate records.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'CurrencyRateID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'CurrencyRateDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'CurrencyRateDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the exchange rate was obtained.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'CurrencyRateDate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'FromCurrencyCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'FromCurrencyCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Exchange rate was converted from this currency code.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'FromCurrencyCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'ToCurrencyCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'ToCurrencyCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Exchange rate was converted to this currency code.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'ToCurrencyCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'AverageRate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'AverageRate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Average exchange rate for the day.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'AverageRate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'EndOfDayRate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'EndOfDayRate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Final exchange rate for the day.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'EndOfDayRate';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CurrencyRate', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CurrencyRate',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CurrencyRate',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

