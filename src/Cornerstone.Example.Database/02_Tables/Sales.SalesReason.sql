-- SalesReason

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'SalesReason' AND
            schemas.name = 'Sales'
    )
    CREATE TABLE [Sales].[SalesReason]
    (
        [SalesReasonID] INT IDENTITY (1, 1) NOT NULL,
        [Name] NAME NOT NULL,
        [ReasonType] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesReason', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesReason';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Lookup table of customer purchase reasons.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesReason';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesReason', 'column', 'SalesReasonID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesReason',
                                     @level2type = N'column',
                                     @level2name = 'SalesReasonID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for SalesReason records.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesReason',
                                @level2type = N'column',
                                @level2name = 'SalesReasonID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesReason', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesReason',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Sales reason description.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesReason',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesReason', 'column', 'ReasonType')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesReason',
                                     @level2type = N'column',
                                     @level2name = 'ReasonType';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Category the sales reason belongs to.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesReason',
                                @level2type = N'column',
                                @level2name = 'ReasonType';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Sales', 'table', 'SalesReason', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Sales',
                                     @level1type = N'table',
                                     @level1name = 'SalesReason',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Sales',
                                @level1type = N'table',
                                @level1name = 'SalesReason',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

