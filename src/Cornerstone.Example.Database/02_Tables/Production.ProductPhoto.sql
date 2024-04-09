-- ProductPhoto

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductPhoto' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductPhoto]
    (
        [ProductPhotoID] INT IDENTITY (1, 1) NOT NULL,
        [ThumbNailPhoto] VARBINARY(MAX) NULL,
        [ThumbnailPhotoFileName] NVARCHAR(50) NULL,
        [LargePhoto] VARBINARY(MAX) NULL,
        [LargePhotoFileName] NVARCHAR(50) NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product images.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'ProductPhotoID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ProductPhotoID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Primary key for ProductPhoto records.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ProductPhotoID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'ThumbNailPhoto')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ThumbNailPhoto';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Small image of the product.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ThumbNailPhoto';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'ThumbnailPhotoFileName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ThumbnailPhotoFileName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Small image file name.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ThumbnailPhotoFileName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'LargePhoto')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'LargePhoto';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Large image of the product.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'LargePhoto';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'LargePhotoFileName')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'LargePhotoFileName';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Large image file name.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'LargePhotoFileName';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductPhoto', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

