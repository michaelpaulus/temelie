-- ProductModelProductDescriptionCulture

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductModelProductDescriptionCulture' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductModelProductDescriptionCulture]
    (
        [ProductModelID] INT NOT NULL,
        [ProductDescriptionID] INT NOT NULL,
        [CultureID] NCHAR(6) NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductModelProductDescriptionCulture', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductModelProductDescriptionCulture';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Cross-reference table mapping product descriptions and the language the description is written in.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductModelProductDescriptionCulture';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductModelProductDescriptionCulture', 'column', 'ProductModelID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductModelProductDescriptionCulture',
                                     @level2type = N'column',
                                     @level2name = 'ProductModelID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to ProductModel.ProductModelID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductModelProductDescriptionCulture',
                                @level2type = N'column',
                                @level2name = 'ProductModelID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductModelProductDescriptionCulture', 'column', 'ProductDescriptionID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductModelProductDescriptionCulture',
                                     @level2type = N'column',
                                     @level2name = 'ProductDescriptionID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key. Foreign key to ProductDescription.ProductDescriptionID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductModelProductDescriptionCulture',
                                @level2type = N'column',
                                @level2name = 'ProductDescriptionID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductModelProductDescriptionCulture', 'column', 'CultureID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductModelProductDescriptionCulture',
                                     @level2type = N'column',
                                     @level2name = 'CultureID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Culture identification number. Foreign key to Culture.CultureID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductModelProductDescriptionCulture',
                                @level2type = N'column',
                                @level2name = 'CultureID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductModelProductDescriptionCulture', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductModelProductDescriptionCulture',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductModelProductDescriptionCulture',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

