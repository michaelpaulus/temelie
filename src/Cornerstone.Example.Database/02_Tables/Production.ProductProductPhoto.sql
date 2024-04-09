-- ProductProductPhoto

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductProductPhoto' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductProductPhoto]
    (
        [ProductID] INT NOT NULL,
        [ProductPhotoID] INT NOT NULL,
        [Primary] FLAG NOT NULL DEFAULT (0),
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO

IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductProductPhoto', DEFAULT, DEFAULT)
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductProductPhoto';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Cross-reference table mapping products and product photos.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductProductPhoto';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductProductPhoto', 'column', 'ProductID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ProductID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product identification number. Foreign key to Product.ProductID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ProductID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductProductPhoto', 'column', 'ProductPhotoID')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ProductPhotoID';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Product photo identification number. Foreign key to ProductPhoto.ProductPhotoID.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ProductPhotoID';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductProductPhoto', 'column', 'Primary')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'Primary';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'0 = Photo is not the principal image. 1 = Photo is the principal image.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductProductPhoto',
                                @level2type = N'column',
                                @level2name = 'Primary';
GO


IF EXISTS
    (
        SELECT
            1
        FROM
            fn_listextendedproperty('MS_Description', 'schema', 'Production', 'table', 'ProductProductPhoto', 'column', 'ModifiedDate')
    )
BEGIN
    EXEC sys.sp_dropextendedproperty @name = N'MS_Description',
                                     @level0type = N'schema',
                                     @level0name = 'Production',
                                     @level1type = N'table',
                                     @level1name = 'ProductProductPhoto',
                                     @level2type = N'column',
                                     @level2name = 'ModifiedDate';
END
GO

EXEC sys.sp_addextendedproperty @name = N'MS_Description',
                                @value = N'Date and time the record was last updated.',
                                @level0type = N'schema',
                                @level0name = 'Production',
                                @level1type = N'table',
                                @level1name = 'ProductProductPhoto',
                                @level2type = N'column',
                                @level2name = 'ModifiedDate';
GO

