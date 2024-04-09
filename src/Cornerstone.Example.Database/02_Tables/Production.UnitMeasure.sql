-- UnitMeasure

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'UnitMeasure' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[UnitMeasure]
    (
        [UnitMeasureCode] NCHAR(3) NOT NULL,
        [Name] NAME NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'UnitMeasure', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'UnitMeasure';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unit of measure lookup table.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'UnitMeasure';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'UnitMeasure', 'column', 'UnitMeasureCode')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'UnitMeasure',
                                     @level2type = N'column',
                                     @level2name = 'UnitMeasureCode';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'UnitMeasure',
                                @level2type = N'column',
                                @level2name = 'UnitMeasureCode';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'UnitMeasure', 'column', 'Name')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'UnitMeasure',
                                     @level2type = N'column',
                                     @level2name = 'Name';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Unit of measure description.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'UnitMeasure',
                                @level2type = N'column',
                                @level2name = 'Name';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'UnitMeasure', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'UnitMeasure',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'UnitMeasure',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

