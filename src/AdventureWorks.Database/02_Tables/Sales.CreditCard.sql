-- CreditCard

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'CreditCard' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[CreditCard]
    (
        [CreditCardID] INT IDENTITY (1, 1) NOT NULL,
        [CardType] NVARCHAR(50) NOT NULL,
        [CardNumber] NVARCHAR(25) NOT NULL,
        [ExpMonth] TINYINT NOT NULL,
        [ExpYear] SMALLINT NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Customer credit card information.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'CreditCardID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'CreditCardID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for CreditCard records.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'CreditCardID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'CardType')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'CardType';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Credit card name.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'CardType';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'CardNumber')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'CardNumber';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Credit card number.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'CardNumber';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'ExpMonth')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'ExpMonth';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Credit card expiration month.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'ExpMonth';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'ExpYear')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'ExpYear';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Credit card expiration year.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'ExpYear';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'CreditCard', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'CreditCard',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'CreditCard',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

